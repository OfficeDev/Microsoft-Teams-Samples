import { HubConnectionState } from "@microsoft/signalr";
import { NegotiateError, REPLACES_HEADER } from "./negotiate";
import { config } from "./config";
import { SocketSession } from "./socketClient";
import { logEvent, logError } from "./log";
import {
  describeConnectError,
  drainAndClose,
  nextLabel,
  openFresh,
  retire,
  seconds,
  regionSockets,
  setCloseListener,
  sleep,
} from "./socketPool";

// Keeps one socket open per region for as long as the bot runs, the way the Teams SDKs should
// (spec "Teams SDK changes for replacing a socket"):
//   * Refresh: 60 s before a socket's token expires (expiresIn - 60 s from its connect), open a new socket with
//     x-ms-socket-replaces-connection-id naming the old one, then drain the old socket (keep answering on it;
//     close it once nothing is in flight and 5 s have passed). A 429 or other failure keeps the old socket
//     serving and retries; if the old socket closes first, the region reconnects without the header.
//   * Reconnect: when a region's socket closes without being replaced (server close, token expiry, network),
//     connect again without the header, backing off 1 s .. 30 s (Retry-After on 429). A 401/403 means socket
//     mode is off for the bot, so the region stops (the SDKs do the same).
//   * Each region is kept independently, so a problem in one region doesn't affect the others.
//   * A replaced socket that closes, whether the bot or APX closed it, is never reconnected.

// Refresh this long before the token expires (expiresIn from the connect response).
const REFRESH_LEAD_MS = 60 * 1000;

const MAX_BACKOFF_MS = 30000;

const keepers: RegionKeeper[] = [];

export function startKeepAlive(): void {
  setCloseListener((session, error) => {
    for (const keeper of keepers) {
      keeper.onSocketClosed(session, error);
    }
  });
  logEvent("keepalive", {
    phase: "start",
    regions: config.regions.join(","),
    refresh: `${REFRESH_LEAD_MS / 1000}s before each token expires (expiresIn from the connect response)`,
    refreshMethod: `new socket with ${REPLACES_HEADER}, then drain and close the old one`,
  });
  for (const region of config.regions) {
    const keeper = new RegionKeeper(region);
    keepers.push(keeper);
    keeper.adopt(regionSockets.get(region));
  }
}

class RegionKeeper {
  private current: SocketSession | undefined;
  private timer: NodeJS.Timeout | undefined;
  private busy = false;
  private stopped = false;
  private backoffMs = 0;

  constructor(private readonly region: string) {}

  adopt(session: SocketSession | undefined): void {
    if (!session || session.connection.state !== HubConnectionState.Connected) {
      void this.reconnect("the region has no open socket");
      return;
    }
    this.current = session;
    this.scheduleRefresh();
  }

  onSocketClosed(session: SocketSession, error?: Error): void {
    // Only the region's current socket matters: a replaced socket is never reconnected, and a socket the bot
    // closed itself was closed on purpose.
    if (session !== this.current || session.closedByBot || this.stopped) {
      return;
    }
    this.clearTimer();
    if (this.busy) {
      // A refresh is in progress; it reconnects if it can't replace the closed socket.
      return;
    }
    const closedAt = session.closedAtMs ?? Date.now();
    const vsExpiryMs = closedAt - session.negotiate.tokenExpiresAtMs;
    const atExpiry = Math.abs(vsExpiryMs) <= 2 * 60 * 1000;
    void this.reconnect(
      `${session.label} closed by the server ${atExpiry ? "at token expiry" : "before token expiry"} ` +
        `(${seconds(vsExpiryMs)}s vs exp, error: ${error?.message ?? "none"})`
    );
  }

  private scheduleRefresh(): void {
    this.clearTimer();
    const session = this.current;
    if (!session || this.stopped) {
      return;
    }
    // expiresIn - 60 s after the connect returned. A lifetime under 2 min (never sent by APX) refreshes at half-life
    // instead, so the refresh never fires immediately and loops.
    const lifetimeMs = session.negotiate.expiresIn * 1000;
    const refreshAt = session.negotiate.negotiatedAtMs + Math.max(lifetimeMs / 2, lifetimeMs - REFRESH_LEAD_MS);
    logEvent("keepalive", {
      region: this.region,
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
        step: `refresh ${this.region}`,
        action: `open ${label} with ${REPLACES_HEADER}: ${old.connectionId} (replacing ${old.label})`,
        reason: `${REFRESH_LEAD_MS / 1000}s before ${old.label}'s token expires`,
      });
      try {
        fresh = await openFresh(index, label, { region: this.region, replacesConnectionId: old.connectionId });
      } catch (e) {
        if (e instanceof NegotiateError && (e.status === 401 || e.status === 403)) {
          this.stop(`refresh refused with ${e.status}: socket mode is off for this bot`, e.cv);
          return;
        }
        const retryMs = e instanceof NegotiateError && e.status === 429 ? (e.retryAfterSec ?? 15) * 1000 : this.nextBackoff();
        logError(
          "keepalive",
          { region: this.region, event: "refresh failed", error: describeConnectError(e), keepServingOn: old.label, retryInMs: retryMs },
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
        region: this.region,
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
        logEvent("flow", { step: `reconnect ${this.region}`, attempt, reason, action: `open ${label} (no header)` });
        try {
          const session = await openFresh(index, label, { region: this.region });
          regionSockets.set(this.region, session);
          this.current = session;
          this.backoffMs = 0;
          logEvent("keepalive", { region: this.region, event: "reconnected", conn: label, connectionId: session.connectionId, attempts: attempt });
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
            { region: this.region, event: "reconnect failed", attempt, error: describeConnectError(e), retryInMs: waitMs },
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
    logError("keepalive", { region: this.region, event: "region stopped", reason, note: "the bot no longer refreshes or reconnects this region; restart it after socket mode is back on" }, cv);
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
