import { HubConnectionState } from "@microsoft/signalr";
import { handleActivity } from "./handler";
import { NegotiateError } from "./negotiate";
import { openDuplicateSocket, openSocket, OpenOptions, SocketCallbacks, SocketSession } from "./socketClient";
import { logEvent, logError } from "./log";

// Shared state for the bot's sockets: which socket serves each slot, the inbound delivery handler, close
// handling, the bounded drain of a replaced socket and the same-token check. Used by the startup self-test
// (selfTest.ts) and by the keep-alive that refreshes and reconnects sockets (keepAlive.ts).

// Number of sockets the bot keeps open (APX's default per-bot limit is 3).
export const TARGET_CONNECTIONS = positiveInt(process.env.SOCKET_CONNECTIONS, 3);

// Startup self-test (limit probe, refresh scenarios) and the same-token check after every new socket.
// SOCKET_SELF_TEST=false skips both and just keeps the sockets open.
export const SELF_TEST_ENABLED = (process.env.SOCKET_SELF_TEST || "true").toLowerCase() !== "false";

// APX caches a bot's connection list for 5s (SignalRSocketConnectionResolver); steps that change the set of
// sockets wait a little longer than that before the next one.
export const STEP_GAP_MS = 6000;

// Generous because a connect can transiently fail (e.g. a 503 from an instance that can't serve it).
const MAX_INITIAL_CONNECT_ATTEMPTS = 20;

// How long to wait for APX to close a second socket opened on an already-used token.
const DUPLICATE_WAIT_MS = 10000;

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
// The socket currently serving each slot (index = slot - 1).
export const slots: SocketSession[] = [];
// Step results for the self-test summary.
export const results: { step: string; expected: string; actual: string; asExpected: boolean }[] = [];

const expiryTimers = new Map<SocketSession, NodeJS.Timeout[]>();
// Deadline (arrival + deadlineMs) of each envelope still in flight, per socket, for the drain.
const inFlightDeadlines = new Map<SocketSession, Map<string, number>>();
const receivedAfterRetire = new Map<SocketSession, number>();
let nextIndex = 1;
let totalEnvelopes = 0;
let closeListener: ((session: SocketSession, error?: Error) => void) | undefined;

export const callbacks: SocketCallbacks = { onActivity, onClosed };

// Lets the keep-alive react to a socket closing (to reconnect its slot).
export function setCloseListener(listener: (session: SocketSession, error?: Error) => void): void {
  closeListener = listener;
}

export function nextLabel(): { index: number; label: string } {
  const index = nextIndex++;
  return { index, label: `conn${index}` };
}

// Opens a new socket and, with the self-test on, checks that APX refuses a second socket on the same token.
export async function openFresh(index: number, label: string, options: OpenOptions): Promise<SocketSession> {
  const session = await openSocket(index, label, callbacks, options);
  track(session);
  if (SELF_TEST_ENABLED) {
    await sameTokenCheck(session);
  }
  return session;
}

// Opens TARGET_CONNECTIONS sockets one after the other; each waits for APX's SocketReady (with the connection
// id) before the next connect starts. Returns false if a socket can't be opened.
export async function openInitialSockets(): Promise<boolean> {
  logEvent("flow", { phase: `open ${TARGET_CONNECTIONS} sockets one after the other` });
  for (let slot = 1; slot <= TARGET_CONNECTIONS; slot++) {
    const session = await openWithRetry(slot);
    if (!session) {
      return false;
    }
    slots[slot - 1] = session;
  }
  logEvent("test", { phase: "sockets open", live: liveCount(), connections: describeSlots() });
  return true;
}

async function openWithRetry(slot: number): Promise<SocketSession | undefined> {
  const { index, label } = nextLabel();
  let backoffMs = 0;
  for (let attempt = 1; attempt <= MAX_INITIAL_CONNECT_ATTEMPTS; attempt++) {
    logEvent("flow", { step: `open ${label}`, slot, attempt, liveBefore: liveCount() });
    try {
      const session = await openFresh(index, label, { slot });
      logEvent("test", { phase: "opened", conn: label, slot, connectionId: session.connectionId, liveNow: liveCount() });
      return session;
    } catch (e) {
      if (attempt === MAX_INITIAL_CONNECT_ATTEMPTS) {
        logError("test", { phase: "giving up", conn: label, attempts: attempt, error: describeConnectError(e) });
        break;
      }
      let waitMs: number;
      if (e instanceof NegotiateError && e.status === 429) {
        waitMs = (e.retryAfterSec ?? 15) * 1000;
        logError(
          "test",
          {
            phase: "connect refused (429)",
            conn: label,
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
        logError("test", { phase: "connect failed", conn: label, attempt, maxAttempts: MAX_INITIAL_CONNECT_ATTEMPTS, retryInMs: waitMs, error: describeConnectError(e) });
      }
      await sleep(waitMs);
    }
  }
  return undefined;
}

// APX allows one socket per token: its hub aborts a socket whose token another socket already uses, before
// that socket joins the bot's group, and logs "BotHub connect REJECTED". The first socket keeps serving.
export async function sameTokenCheck(primary: SocketSession): Promise<void> {
  const dupLabel = `${primary.label}-dup`;
  const step = `same-token ${primary.label}`;
  const expected = "2nd socket closed by APX without SocketReady; 1st socket stays open";
  logEvent("flow", {
    step,
    action: `open ${dupLabel} with the same url + access token as ${primary.label}`,
    tokenId: primary.negotiate.tokenId,
    primaryConnectionId: primary.connectionId,
    expected: `${expected} (APX log: BotHub connect REJECTED ... already uses this token)`,
  });
  const r = await openDuplicateSocket(primary, dupLabel, (env) => handleActivity(env), DUPLICATE_WAIT_MS);
  // Give a wrongly closed primary a moment to show up as closed.
  await sleep(1000);
  const primaryState = primary.connection.state;
  const rejectedByServer = !r.gotSocketReady && !r.stoppedByBot && (r.closed || !!r.startError);
  const asExpected = rejectedByServer && primaryState === HubConnectionState.Connected;
  const actual = rejectedByServer
    ? `2nd socket ${r.startError ? "failed to start" : "closed by server"} after ${r.closedAfterMs}ms; 1st socket ${primaryState}`
    : `2nd socket ${r.gotSocketReady ? "got SocketReady" : "stayed open"} (stopped by bot); 1st socket ${primaryState}`;
  record(step, expected, actual, asExpected);
  (asExpected ? logEvent : logError)("same-token <<", {
    conn: primary.label,
    dup: dupLabel,
    tokenId: primary.negotiate.tokenId,
    dupStarted: r.started,
    dupStartError: r.startError,
    dupClientConnectionId: r.clientConnectionId,
    dupGotSocketReady: r.gotSocketReady,
    dupDeliveries: r.deliveries,
    dupClosedBy: r.stoppedByBot ? "bot (APX left it open)" : rejectedByServer ? "server" : "-",
    dupClosedAfterMs: r.closedAfterMs,
    dupCloseError: r.closeError ?? r.startError ?? "(none)",
    primaryConnectionId: primary.connectionId,
    primaryState,
    asExpected,
  });
}

// Marks `old` as replaced by `fresh` in its slot.
export function retire(old: SocketSession, fresh: SocketSession, mode: "legacy" | "header"): void {
  old.retiredAtMs = fresh.readyAtMs;
  old.retiredBy = fresh.label;
  old.retiredMode = mode;
  slots[fresh.slot - 1] = fresh;
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

export function record(step: string, expected: string, actual: string, asExpected: boolean): void {
  results.push({ step, expected, actual, asExpected });
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
      slot: s.slot,
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
  logEvent("status", { reason, live: liveCount(), slots: describeSlots(), envelopes: totalEnvelopes, tally: tally() });
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
      slot: session.slot,
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
    slot: session.slot,
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
}

// Logs when the token's exp is reached and, if the socket is still open shortly after, that the server
// did not close it at expiry.
function scheduleExpiryWatch(session: SocketSession): void {
  const untilExpiry = Math.max(0, session.negotiate.tokenExpiresAtMs - Date.now());
  const atExpiry = setTimeout(() => {
    logEvent("expiry", { conn: session.label, connectionId: session.connectionId, event: "token exp reached", state: session.connection.state });
  }, untilExpiry);
  const afterExpiry = setTimeout(() => {
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
  }, untilExpiry + EXPIRY_TOLERANCE_MS);
  expiryTimers.set(session, [atExpiry, afterExpiry]);
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

export function receivedWhileRetired(session: SocketSession): number {
  return receivedAfterRetire.get(session) ?? 0;
}

// Open sockets the bot knows of.
export function liveCount(): number {
  return sessions.filter((s) => s.connection.state === HubConnectionState.Connected).length;
}

// Open sockets still in the bot's group as APX sees it: everything open except old sockets of a header refresh,
// which APX moved to the draining group.
export function inGroupCount(): number {
  return sessions.filter((s) => s.connection.state === HubConnectionState.Connected && s.retiredMode !== "header").length;
}

export function drainingCount(): number {
  return sessions.filter((s) => s.connection.state === HubConnectionState.Connected && s.retiredMode === "header").length;
}

export function describeSlots(): string {
  return slots.map((s) => `slot${s.slot}=${s.label}(${s.connectionId}, ${s.connection.state})`).join(" ");
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

export function positiveInt(raw: string | undefined, fallback: number): number {
  const n = Number(raw);
  return Number.isInteger(n) && n > 0 ? n : fallback;
}

export function sleep(ms: number): Promise<void> {
  return new Promise((r) => setTimeout(r, ms));
}
