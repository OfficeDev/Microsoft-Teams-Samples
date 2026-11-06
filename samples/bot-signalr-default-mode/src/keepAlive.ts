import { HubConnectionState } from "@microsoft/signalr";
import { NegotiateError, REPLACES_HEADER } from "./negotiate";
import { SocketSession } from "./socketClient";
import { logEvent, logError } from "./log";
import {
  TARGET_CONNECTIONS,
  describeConnectError,
  drainAndClose,
  nextLabel,
  openFresh,
  positiveInt,
  retire,
  setCloseListener,
  sleep,
  slots,
} from "./socketPool";

// Keeps one socket open per slot for as long as the bot runs, the way the Teams SDKs should
// (spec "Teams SDK changes for replacing a socket"):
//   * Refresh: 60 s before a socket's token expires (expiresIn - 60 s from its connect), open a new socket with
//     x-ms-socket-replaces-connection-id naming the old one, then drain the old socket (keep answering on it;
//     close it once nothing is in flight and 5 s have passed). A 429 or other failure keeps the old socket
//     serving and retries; if the old socket closes first, the slot reconnects without the header.
//   * Reconnect: when a slot's socket closes without being replaced (server close, token expiry, network),
//     connect again without the header, backing off 1 s .. 30 s (Retry-After on 429). A 401/403 means socket
//     mode is off for the bot, so the slot stops (the SDKs do the same).
//   * A replaced socket that closes, whether the bot or APX closed it, is never reconnected.

// Refresh this long before the token expires.
const REFRESH_LEAD_MS = 60 * 1000;

// For trying refreshes without waiting for the token lifetime: refresh each socket this many seconds after it
// is ready. Unset = refresh 60 s before expiry.
const REFRESH_AFTER_SECONDS = positiveInt(process.env.SOCKET_REFRESH_AFTER_SECONDS, 0);

const MAX_BACKOFF_MS = 30000;

const keepers: SlotKeeper[] = [];

export function startKeepAlive(): void {
  setCloseListener((session, error) => {
    for (const keeper of keepers) {
      keeper.onSocketClosed(session, error);
    }
  });
  logEvent("keepalive", {
    phase: "start",
    slots: TARGET_CONNECTIONS,
    refresh: REFRESH_AFTER_SECONDS
      ? `${REFRESH_AFTER_SECONDS}s after each socket is ready (SOCKET_REFRESH_AFTER_SECONDS)`
      : `${REFRESH_LEAD_MS / 1000}s before each token expires`,
    refreshMethod: `new socket with ${REPLACES_HEADER}, then drain and close the old one`,
  });
  for (let slot = 1; slot <= TARGET_CONNECTIONS; slot++) {
    const keeper = new SlotKeeper(slot);
    keepers.push(keeper);
    keeper.adopt(slots[slot - 1]);
  }
}

class SlotKeeper {
  private current: SocketSession | undefined;
  private timer: NodeJS.Timeout | undefined;
  private busy = false;
  private stopped = false;
  private backoffMs = 0;

  constructor(private readonly slot: number) {}

  adopt(session: SocketSession | undefined): void {
    if (!session || session.connection.state !== HubConnectionState.Connected) {
      void this.reconnect("the slot has no open socket");
      return;
    }
    this.current = session;
    this.scheduleRefresh();
  }

  onSocketClosed(session: SocketSession, error?: Error): void {
    // Only the slot's current socket matters: a replaced socket is never reconnected, and a socket the bot
    // closed itself was closed on purpose.
    if (session !== this.current || session.closedByBot || this.stopped) {
      return;
    }
    this.clearTimer();
    if (this.busy) {
      // A refresh is in progress; it reconnects if it can't replace the closed socket.
      return;
    }
    void this.reconnect(`${session.label} closed: ${error?.message ?? "no error"}`);
  }

  private scheduleRefresh(): void {
    this.clearTimer();
    const session = this.current;
    if (!session || this.stopped) {
      return;
    }
    const refreshAt = REFRESH_AFTER_SECONDS
      ? session.readyAtMs + REFRESH_AFTER_SECONDS * 1000
      : session.negotiate.negotiatedAtMs + Math.max(session.negotiate.expiresIn * 500, session.negotiate.expiresIn * 1000 - REFRESH_LEAD_MS);
    logEvent("keepalive", {
      slot: this.slot,
      conn: session.label,
      connectionId: session.connectionId,
      event: "refresh scheduled",
      refreshAt: new Date(refreshAt).toISOString(),
      expiresIn: session.negotiate.expiresIn,
      tokenExpiresAt: new Date(session.negotiate.tokenExpiresAtMs).toISOString(),
    });
    this.timer = setTimeout(() => void this.refresh(), Math.max(0, refreshAt - Date.now()));
  }

  private async refresh(): Promise<void> {
    if (this.busy || this.stopped) {
      return;
    }
    const old = this.current;
    if (!old || old.connection.state !== HubConnectionState.Connected) {
      await this.reconnect("the socket to refresh is no longer open");
      return;
    }
    this.busy = true;
    let fresh: SocketSession | undefined;
    try {
      const { index, label } = nextLabel();
      logEvent("flow", {
        step: `refresh slot ${this.slot}`,
        action: `open ${label} with ${REPLACES_HEADER}: ${old.connectionId} (replacing ${old.label})`,
        reason: REFRESH_AFTER_SECONDS ? `${REFRESH_AFTER_SECONDS}s after ${old.label} was ready` : `${REFRESH_LEAD_MS / 1000}s before ${old.label}'s token expires`,
      });
      try {
        fresh = await openFresh(index, label, { slot: this.slot, replacesConnectionId: old.connectionId });
      } catch (e) {
        if (e instanceof NegotiateError && (e.status === 401 || e.status === 403)) {
          this.stop(`refresh refused with ${e.status}: socket mode is off for this bot`, e.cv);
          return;
        }
        const retryMs = e instanceof NegotiateError && e.status === 429 ? (e.retryAfterSec ?? 15) * 1000 : this.nextBackoff();
        logError(
          "keepalive",
          { slot: this.slot, event: "refresh failed", error: describeConnectError(e), keepServingOn: old.label, retryInMs: retryMs },
          e instanceof NegotiateError ? e.cv : undefined
        );
        this.timer = setTimeout(() => void this.refresh(), retryMs);
        return;
      }
      this.backoffMs = 0;
      retire(old, fresh, "header");
      this.current = fresh;
      logEvent("refresh <<", {
        mode: "header",
        slot: this.slot,
        result: "OK",
        newConn: fresh.label,
        newConnectionId: fresh.connectionId,
        oldConn: old.label,
        oldConnectionId: old.connectionId,
        tokenReplacesClaim: fresh.negotiate.replacesClaim ?? "(absent: the old socket had already left the bot's group)",
      });
      this.scheduleRefresh();
    } finally {
      this.busy = false;
    }
    if (fresh) {
      await drainAndClose(old, fresh);
    }
    // The old socket may have closed while the refresh was failing; reconnect if nothing replaced it.
    if (this.current?.connection.state === HubConnectionState.Disconnected && !this.current.closedByBot) {
      await this.reconnect(`${this.current.label} closed during the refresh`);
    }
  }

  private async reconnect(reason: string): Promise<void> {
    if (this.busy || this.stopped) {
      return;
    }
    this.busy = true;
    this.clearTimer();
    try {
      for (let attempt = 1; !this.stopped; attempt++) {
        const { index, label } = nextLabel();
        logEvent("flow", { step: `reconnect slot ${this.slot}`, attempt, reason, action: `open ${label} (no header)` });
        try {
          const session = await openFresh(index, label, { slot: this.slot });
          slots[this.slot - 1] = session;
          this.current = session;
          this.backoffMs = 0;
          logEvent("keepalive", { slot: this.slot, event: "reconnected", conn: label, connectionId: session.connectionId, attempts: attempt });
          this.scheduleRefresh();
          return;
        } catch (e) {
          if (e instanceof NegotiateError && (e.status === 401 || e.status === 403)) {
            this.stop(`reconnect refused with ${e.status}: socket mode is off for this bot`, e.cv);
            return;
          }
          const waitMs = e instanceof NegotiateError && e.status === 429 ? (e.retryAfterSec ?? 15) * 1000 : this.nextBackoff();
          logError(
            "keepalive",
            { slot: this.slot, event: "reconnect failed", attempt, error: describeConnectError(e), retryInMs: waitMs },
            e instanceof NegotiateError ? e.cv : undefined
          );
          await sleep(waitMs);
        }
      }
    } finally {
      this.busy = false;
    }
  }

  private stop(reason: string, cv?: string): void {
    this.stopped = true;
    this.clearTimer();
    logError("keepalive", { slot: this.slot, event: "slot stopped", reason, note: "the bot no longer refreshes or reconnects this slot; restart it after socket mode is back on" }, cv);
  }

  private nextBackoff(): number {
    this.backoffMs = this.backoffMs === 0 ? 1000 : Math.min(this.backoffMs * 2, MAX_BACKOFF_MS);
    return this.backoffMs;
  }

  private clearTimer(): void {
    if (this.timer) {
      clearTimeout(this.timer);
      this.timer = undefined;
    }
  }
}
