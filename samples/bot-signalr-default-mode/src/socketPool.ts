import { HubConnectionState } from "@microsoft/signalr";
import { config } from "./config";
import { handleActivity } from "./handler";
import { NegotiateError } from "./negotiate";
import { openSocket, OpenOptions, SocketCallbacks, SocketSession } from "./socketClient";
import { logEvent, logError } from "./log";

// Shared state for the bot's sockets, one per region: which socket serves each region, the inbound delivery
// handler, close handling and the bounded drain of a replaced socket. Used by the keep-alive that refreshes and
// reconnects sockets (keepAlive.ts).
// Logged this long before a token expires, so the close that follows is easy to line up.
const EXPIRY_HEADS_UP_MS = 60 * 1000;

// APX caches a bot's connection list for 5s (SignalRSocketConnectionResolver); steps that change the set of
// sockets wait a little longer than that before the next one.
export const STEP_GAP_MS = 6000;

// Generous because a connect can transiently fail (e.g. a 503 from an instance that can't serve it).
const MAX_INITIAL_CONNECT_ATTEMPTS = 20;

// Replacing a socket (spec "Teams SDK changes for replacing a socket"): keep answering on the old socket and
// close it once nothing is in flight there and at least 5 s have passed since the new socket was ready (APX's
// connection cache can still hand out the old socket for that long), but no later than the latest deadline of
// what is still in flight on it.
const DRAIN_MIN_MS = 5000;
const DEFAULT_DEADLINE_MS = 25000;

export const APX_REPLACED_REASON = "Replaced by a newer socket.";

const STATUS_INTERVAL_MS = 5 * 60 * 1000;

// Closed sockets are dropped from the status output this long after they closed.
const CLOSED_SOCKET_RETENTION_MS = 15 * 60 * 1000;

// A server close within this window of the token's exp claim counts as "closed at token expiry".
const EXPIRY_TOLERANCE_MS = 2 * 60 * 1000;

export const sessions: SocketSession[] = [];
// The socket currently serving each region.
export const regionSockets = new Map<string, SocketSession>();

const expiryTimers = new Map<SocketSession, NodeJS.Timeout[]>();
// Deadline (arrival + deadlineMs) of each envelope still in flight, per socket, for the drain.
const inFlightDeadlines = new Map<SocketSession, Map<string, number>>();
const receivedAfterRetire = new Map<SocketSession, number>();
let nextIndex = 1;
let totalEnvelopes = 0;
let closeListener: ((session: SocketSession, error?: Error) => void) | undefined;

export const callbacks: SocketCallbacks = { onActivity, onClosed };

// Lets the keep-alive react to a socket closing (to reconnect its region).
export function setCloseListener(listener: (session: SocketSession, error?: Error) => void): void {
  closeListener = listener;
}

export function nextLabel(): { index: number; label: string } {
  const index = nextIndex++;
  return { index, label: `conn${index}` };
}

// Opens a new socket and starts tracking it.
export async function openFresh(index: number, label: string, options: OpenOptions): Promise<SocketSession> {
  const session = await openSocket(index, label, callbacks, options);
  track(session);
  return session;
}

// Opens one socket in each region, all regions at once; each socket waits for APX's SocketReady (with the
// connection id). A region that can't open is left to the keep-alive, which keeps reconnecting it, so a problem in
// one region doesn't affect the others. Returns false only if no region could be opened.
export async function openInitialSockets(): Promise<boolean> {
  logEvent("flow", { phase: `open one socket per region: ${config.regions.join(", ")}` });
  const opened = await Promise.all(config.regions.map((region) => openWithRetry(region)));
  config.regions.forEach((region, i) => {
    const session = opened[i];
    if (session) {
      regionSockets.set(region, session);
    }
  });
  logEvent("flow", { phase: "sockets open", live: liveCount(), connections: describeRegions() });
  return regionSockets.size > 0;
}

async function openWithRetry(region: string): Promise<SocketSession | undefined> {
  const { index, label } = nextLabel();
  let backoffMs = 0;
  for (let attempt = 1; attempt <= MAX_INITIAL_CONNECT_ATTEMPTS; attempt++) {
    logEvent("flow", { step: `open ${label}`, region, attempt, liveBefore: liveCount() });
    try {
      const session = await openFresh(index, label, { region });
      logEvent("flow", { phase: "opened", conn: label, region, connectionId: session.connectionId, liveNow: liveCount() });
      return session;
    } catch (e) {
      if (attempt === MAX_INITIAL_CONNECT_ATTEMPTS) {
        logError("flow", { phase: "giving up for now; the keep-alive keeps reconnecting", conn: label, region, attempts: attempt, error: describeConnectError(e) });
        break;
      }
      let waitMs: number;
      if (e instanceof NegotiateError && e.status === 429) {
        waitMs = (e.retryAfterSec ?? 15) * 1000;
        logError(
          "flow",
          {
            phase: "connect refused (429)",
            conn: label,
            region,
            attempt,
            liveBefore: liveCount(),
            note: "APX still counts other sockets for this bot (e.g. another bot instance); waiting Retry-After",
            retryInMs: waitMs,
          },
          e.cv
        );
      } else {
        backoffMs = backoffMs === 0 ? 1000 : Math.min(backoffMs * 2, 30000);
        waitMs = backoffMs;
        logError("flow", { phase: "connect failed", conn: label, region, attempt, maxAttempts: MAX_INITIAL_CONNECT_ATTEMPTS, retryInMs: waitMs, error: describeConnectError(e) });
      }
      await sleep(waitMs);
    }
  }
  return undefined;
}

// Marks `old` as replaced by `fresh` in its region.
export function retire(old: SocketSession, fresh: SocketSession, mode: "header"): void {
  old.retiredAtMs = fresh.readyAtMs;
  old.retiredBy = fresh.label;
  old.retiredMode = mode;
  regionSockets.set(fresh.region, fresh);
}

// Keep answering on the old socket; close it once nothing is in flight there and at least 5 s have passed
// since the new socket was ready, but no later than the latest deadline still in flight.
export async function drainAndClose(old: SocketSession, fresh: SocketSession): Promise<void> {
  const earliestCloseAt = fresh.readyAtMs + DRAIN_MIN_MS;
  logEvent("drain", { conn: old.label, event: "start", inFlight: old.inFlight, earliestCloseAt: new Date(earliestCloseAt).toISOString() });
  let reason = "";
  while (!reason) {
    const now = Date.now();
    if (old.connection.state === HubConnectionState.Disconnected) {
      reason = "already closed by the server";
    } else if (now >= earliestCloseAt && old.inFlight === 0) {
      reason = `nothing in flight, ${seconds(now - fresh.readyAtMs)}s after ${fresh.label} was ready`;
    } else if (now >= earliestCloseAt && latestDeadline(old) <= now) {
      reason = `latest in-flight deadline passed (${old.inFlight} still in flight)`;
    } else {
      await sleep(250);
    }
  }
  logEvent("drain", { conn: old.label, event: "done", reason, receivedWhileDraining: receivedAfterRetire.get(old) ?? 0 });
  if (old.connection.state !== HubConnectionState.Disconnected) {
    await closeByBot(old, `drained, replaced by ${fresh.label}`);
  }
}

export async function closeByBot(session: SocketSession, reason: string): Promise<void> {
  logEvent("flow", {
    step: `close ${session.label}`,
    reason,
    connectionId: session.connectionId,
    inFlight: session.inFlight,
    received: session.received,
    receivedAfterRetire: session.retiredAtMs ? receivedAfterRetire.get(session) ?? 0 : undefined,
  });
  session.closedByBot = true;
  await session.connection.stop().catch((e) => logError("socket", { conn: session.label, event: "stop failed", error: (e as Error).message }));
}

export function track(session: SocketSession): void {
  sessions.push(session);
  // The socket may already have closed while the connect was finishing.
  if (session.connection.state !== HubConnectionState.Disconnected) {
    scheduleExpiryWatch(session);
  }
}

export function startStatusTimer(): void {
  logStatus("ready");
  setInterval(() => logStatus("periodic"), STATUS_INTERVAL_MS);
}

// Logs every open socket (and recently closed ones) plus the delivery tally, e.g. every 5 minutes and at exit.
export function logStatus(reason: string): void {
  const now = Date.now();
  for (let i = sessions.length - 1; i >= 0; i--) {
    const closedAt = sessions[i].closedAtMs;
    if (closedAt && now - closedAt > CLOSED_SOCKET_RETENTION_MS) {
      sessions.splice(i, 1);
    }
  }
  for (const s of sessions) {
    logEvent("status", {
      reason,
      conn: s.label,
      region: s.region,
      connectionId: s.connectionId,
      state: s.connection.state,
      openedAt: s.connectedAtMs ? new Date(s.connectedAtMs).toISOString() : undefined,
      openMin: s.connectedAtMs ? minutes((s.closedAtMs ?? now) - s.connectedAtMs) : undefined,
      tokenExpiresAt: new Date(s.negotiate.tokenExpiresAtMs).toISOString(),
      tokenExpiresInMin: minutes(s.negotiate.tokenExpiresAtMs - now),
      retired: s.retiredAtMs ? `${s.retiredMode} by ${s.retiredBy}` : undefined,
      received: s.received,
      inFlight: s.inFlight,
      closedAt: s.closedAtMs ? new Date(s.closedAtMs).toISOString() : undefined,
      closedBy: s.closedAtMs ? (s.closedByBot ? "bot" : "server/network") : undefined,
      closeError: s.closeError,
    });
  }
  logEvent("status", { reason, live: liveCount(), regions: describeRegions(), envelopes: totalEnvelopes, tally: tally() });
}

// ---------------------------------------------------------------------------------------------------------
// Socket callbacks

async function onActivity(session: SocketSession, envelope: any): Promise<unknown> {
  session.received += 1;
  session.inFlight += 1;
  totalEnvelopes += 1;
  if (session.retiredAtMs) {
    receivedAfterRetire.set(session, (receivedAfterRetire.get(session) ?? 0) + 1);
  }
  const payload = envelope?.payload ?? envelope?.Payload;
  const envelopeId = String(envelope?.envelopeId ?? envelope?.EnvelopeId ?? `local-${totalEnvelopes}`);
  const cv = envelope?.cv ?? envelope?.Cv;
  const deadlineMs = Number(envelope?.deadlineMs ?? envelope?.DeadlineMs ?? DEFAULT_DEADLINE_MS);
  const startedAt = Date.now();
  deadlinesOf(session).set(envelopeId, startedAt + deadlineMs);
  // After a header refresh APX stops routing to the old socket, but its 5 s connection cache can still hand it
  // out briefly; only a delivery after that window is unexpected.
  const sinceRetireMs = session.retiredAtMs ? startedAt - session.retiredAtMs : undefined;
  const unexpectedOnRetired = session.retiredMode === "header" && sinceRetireMs !== undefined && sinceRetireMs > STEP_GAP_MS;
  (unexpectedOnRetired ? logError : logEvent)(
    "route <<",
    {
      conn: session.label,
      region: session.region,
      connectionId: session.connectionId,
      retired: session.retiredAtMs ? `${session.retiredMode} ${seconds(sinceRetireMs ?? 0)}s ago by ${session.retiredBy}` : undefined,
      note: unexpectedOnRetired
        ? "UNEXPECTED: APX still routes to a socket it replaced, after its 5 s cache"
        : session.retiredMode === "header"
          ? "within APX's 5 s connection cache after the replace; possible"
          : undefined,
      envelopeNo: totalEnvelopes,
      envelopeId,
      envelopeType: envelope?.type ?? envelope?.Type,
      activityType: payload?.type ?? payload?.Type,
      name: payload?.name ?? payload?.Name,
      deadlineMs,
      inFlightOnConn: session.inFlight,
      live: liveCount(),
      tally: tally(),
    },
    cv
  );
  try {
    const reply = (await handleActivity(envelope)) as { status?: number } | undefined;
    logEvent("route >>", { conn: session.label, connectionId: session.connectionId, envelopeId, replyStatus: reply?.status, handlerMs: Date.now() - startedAt }, cv);
    return reply;
  } catch (e) {
    logError("route >>", { conn: session.label, connectionId: session.connectionId, envelopeId, result: "handler threw", error: (e as Error).message }, cv);
    throw e;
  } finally {
    session.inFlight -= 1;
    deadlinesOf(session).delete(envelopeId);
  }
}

function onClosed(session: SocketSession, error?: Error): void {
  for (const t of expiryTimers.get(session) ?? []) {
    clearTimeout(t);
  }
  expiryTimers.delete(session);

  const closedAt = session.closedAtMs ?? Date.now();
  const expiresAt = session.negotiate.tokenExpiresAtMs;
  let verdict: string;
  if (session.closedByBot) {
    verdict = "closed by the bot";
  } else if (session.retiredAtMs) {
    verdict = (error?.message ?? "").includes(APX_REPLACED_REASON)
      ? `CLOSED BY APX AFTER REPLACE (${seconds(closedAt - session.retiredAtMs)}s after ${session.retiredBy} was ready)`
      : `retired socket closed by the server ${seconds(closedAt - session.retiredAtMs)}s after ${session.retiredBy} was ready`;
  } else if (Math.abs(closedAt - expiresAt) <= EXPIRY_TOLERANCE_MS) {
    verdict = "CLOSED AT TOKEN EXPIRY";
  } else if (closedAt < expiresAt) {
    verdict = "CLOSED BEFORE TOKEN EXPIRY (not the expiry)";
  } else {
    verdict = "CLOSED LATE (more than 2 min after token expiry)";
  }

  (session.closedByBot || session.retiredAtMs ? logEvent : logError)("socket", {
    conn: session.label,
    region: session.region,
    event: "closed",
    connectionId: session.connectionId || "(before SocketReady)",
    closedBy: session.closedByBot ? "bot" : "server/network",
    error: error?.message ?? "(none)",
    openSec: session.connectedAtMs ? Math.round((closedAt - session.connectedAtMs) / 1000) : undefined,
    expiresIn: session.negotiate.expiresIn,
    tokenExpiresAt: new Date(expiresAt).toISOString(),
    closedVsTokenExpirySec: Math.round((closedAt - expiresAt) / 1000),
    verdict,
    received: session.received,
    receivedAfterRetire: session.retiredAtMs ? receivedAfterRetire.get(session) ?? 0 : undefined,
    liveNow: liveCount(),
  });

  closeListener?.(session, error);

  // One [expiry] line per server-side close of a socket that wasn't replaced, to grep the expiry results.
  if (!session.closedByBot && !session.retiredAtMs) {
    const nearExpiry = Math.abs(closedAt - expiresAt) <= EXPIRY_TOLERANCE_MS;
    (nearExpiry ? logEvent : logError)("expiry", {
      conn: session.label,
      connectionId: session.connectionId,
      event: "closed by server",
      expiresIn: session.negotiate.expiresIn,
      tokenExpiresAt: new Date(expiresAt).toISOString(),
      closedAt: new Date(closedAt).toISOString(),
      closedVsTokenExpirySec: seconds(closedAt - expiresAt),
      openSec: session.connectedAtMs ? seconds(closedAt - session.connectedAtMs) : undefined,
      closeError: error?.message ?? "(none)",
      verdict,
    });
  }
}

// Logs the expected close time when a socket opens, a heads-up 60 s before its token expires, when exp is
// reached and, if the socket is still open shortly after, that the server did not close it at expiry.
function scheduleExpiryWatch(session: SocketSession): void {
  const untilExpiry = Math.max(0, session.negotiate.tokenExpiresAtMs - Date.now());
  logEvent("expiry", {
    conn: session.label,
    connectionId: session.connectionId,
    event: "watching",
    expiresIn: session.negotiate.expiresIn,
    lifetimeMin: minutes(session.negotiate.expiresIn * 1000),
    tokenExpiresAt: new Date(session.negotiate.tokenExpiresAtMs).toISOString(),
    expect: "refreshed before tokenExpiresAt",
  });
  const timers: NodeJS.Timeout[] = [];
  if (untilExpiry > EXPIRY_HEADS_UP_MS) {
    timers.push(
      setTimeout(() => {
        logEvent("expiry", { conn: session.label, connectionId: session.connectionId, event: "token expires in 60s", state: session.connection.state });
      }, untilExpiry - EXPIRY_HEADS_UP_MS)
    );
  }
  timers.push(
    setTimeout(() => {
      logEvent("expiry", { conn: session.label, connectionId: session.connectionId, event: "token exp reached", state: session.connection.state });
    }, untilExpiry)
  );
  timers.push(
    setTimeout(() => {
      if (session.connection.state !== HubConnectionState.Disconnected) {
        logError("expiry", {
          conn: session.label,
          connectionId: session.connectionId,
          event: "STILL OPEN after token expiry",
          state: session.connection.state,
          minutesPastExpiry: minutes(Date.now() - session.negotiate.tokenExpiresAtMs),
          verdict: "server did NOT close the socket at token expiry",
        });
      }
    }, untilExpiry + EXPIRY_TOLERANCE_MS)
  );
  expiryTimers.set(session, timers);
}

// ---------------------------------------------------------------------------------------------------------
// Helpers

export function describeConnectError(e: unknown): string {
  if (e instanceof NegotiateError) {
    return `${e.status}${e.retryAfterSec !== undefined ? ` Retry-After ${e.retryAfterSec}` : ""} ${e.body}`.trim();
  }
  return (e as Error).message;
}

function deadlinesOf(session: SocketSession): Map<string, number> {
  let m = inFlightDeadlines.get(session);
  if (!m) {
    m = new Map();
    inFlightDeadlines.set(session, m);
  }
  return m;
}

function latestDeadline(session: SocketSession): number {
  const values = [...deadlinesOf(session).values()];
  return values.length ? Math.max(...values) : 0;
}

// Open sockets the bot knows of.
export function liveCount(): number {
  return sessions.filter((s) => s.connection.state === HubConnectionState.Connected).length;
}

export function describeRegions(): string {
  return [...regionSockets.values()].map((s) => `${s.region}=${s.label}(${s.connectionId}, ${s.connection.state})`).join(" ");
}

function tally(): string {
  return sessions.map((s) => `${s.label}:${s.received}`).join(" ");
}

export function minutes(ms: number): string {
  return (ms / 60000).toFixed(1);
}

export function seconds(ms: number): string {
  return (ms / 1000).toFixed(1);
}

export function sleep(ms: number): Promise<void> {
  return new Promise((r) => setTimeout(r, ms));
}
