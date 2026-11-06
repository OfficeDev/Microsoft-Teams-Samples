import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  ILogger,
  LogLevel,
} from "@microsoft/signalr";
import { negotiate, NegotiateResult } from "./negotiate";
import { logEvent, logError } from "./log";

// How long to wait after the socket opens for APX's SocketReady frame (sent from BotHub.OnConnectedAsync
// once the connection has joined the bot's group).
const SOCKET_READY_TIMEOUT_MS = 30000;

// One socket opened by the bot: an APX connect (negotiate) plus the Azure SignalR connection it led to.
export interface SocketSession {
  index: number;
  label: string;
  // Which of the long-lived slots this socket serves (1..N); refreshes keep the slot and change the socket.
  slot: number;
  connection: HubConnection;
  negotiate: NegotiateResult;
  connectedAtMs: number;
  readyAtMs: number;
  // The hub connection id APX reported in SocketReady. APX routes deliveries to this id.
  connectionId: string;
  // The id the client library got from Azure SignalR's negotiate. Logged to confirm it matches.
  clientConnectionId?: string;
  // Set when a refresh opened with the replaces header names this socket.
  replacesConnectionId?: string;
  received: number;
  inFlight: number;
  closedAtMs?: number;
  closeError?: string;
  // True when the bot itself stopped the socket, so the close isn't mistaken for a server close.
  closedByBot: boolean;
  // Set once a newer socket took over this socket's slot.
  retiredAtMs?: number;
  retiredBy?: string;
  retiredMode?: "legacy" | "header";
}

export interface SocketCallbacks {
  onActivity: (session: SocketSession, envelope: any) => Promise<unknown>;
  onClosed: (session: SocketSession, error?: Error) => void;
}

export interface OpenOptions {
  slot: number;
  // Sends x-ms-socket-replaces-connection-id with this server connectionId (refresh, new way).
  replacesConnectionId?: string;
}

// Builds a SignalR hub connection to the negotiated Azure SignalR URL and wires the inbound
// "Activity" client method (APX -> bot).
//
// Default mode delivers each invoke via *client results*: APX calls
// `InvokeAsync<InvokeReplyFrame>("Activity", envelope)` and awaits the return value, which the Azure
// SignalR Service routes back to the exact invoking pod by invocation id. So the registered handler
// RETURNS the reply frame and @microsoft/signalr sends it back as the invocation result — there is no
// separate `send`, no upstream webhook, and no correlation bookkeeping on either side.
//
// Resolves once APX's SocketReady has arrived, so the caller knows the connection id APX will route to.
// There is deliberately no automatic reconnect and no token refresh: when Azure SignalR closes the socket
// (e.g. at token expiry) the close is logged and the socket stays closed.
export async function openSocket(
  index: number,
  label: string,
  callbacks: SocketCallbacks,
  options: OpenOptions
): Promise<SocketSession> {
  const neg = await negotiate(label, options.replacesConnectionId);
  const conn = buildHub(neg, label);

  const session: SocketSession = {
    index,
    label,
    slot: options.slot,
    connection: conn,
    negotiate: neg,
    connectedAtMs: 0,
    readyAtMs: 0,
    connectionId: "",
    replacesConnectionId: options.replacesConnectionId,
    received: 0,
    inFlight: 0,
    closedByBot: false,
  };

  // Returning a value (Promise) from this handler is what makes client results work — APX awaits it.
  conn.on("Activity", (envelope: any) => callbacks.onActivity(session, envelope));

  let resolveReady: (frame: any) => void = () => undefined;
  const ready = new Promise<any>((resolve) => (resolveReady = resolve));
  conn.on("SocketReady", (frame: any) => {
    logEvent("SocketReady <<", {
      conn: label,
      dir: "APX->bot",
      protocolVersion: frame?.protocolVersion ?? frame?.ProtocolVersion,
      botKey: frame?.botKey ?? frame?.BotKey,
      connectionId: frame?.connectionId ?? frame?.ConnectionId,
      msSinceSocketOpen: session.connectedAtMs ? Date.now() - session.connectedAtMs : undefined,
    });
    resolveReady(frame);
  });

  conn.onclose((e) => {
    session.closedAtMs = Date.now();
    session.closeError = e?.message;
    callbacks.onClosed(session, e);
  });

  const startAt = Date.now();
  await conn.start();
  session.connectedAtMs = Date.now();
  session.clientConnectionId = conn.connectionId ?? undefined;
  logEvent("socket", {
    conn: label,
    event: "connected",
    note: "Azure SignalR (Default mode)",
    clientConnectionId: session.clientConnectionId,
    startMs: session.connectedAtMs - startAt,
    expiresIn: neg.expiresIn,
    tokenExpiresAt: new Date(neg.tokenExpiresAtMs).toISOString(),
  });

  const frame = await withTimeout(ready, SOCKET_READY_TIMEOUT_MS);
  if (!frame) {
    logError("socket", { conn: label, event: "SocketReady TIMEOUT", waitedMs: SOCKET_READY_TIMEOUT_MS, note: "stopping this socket" });
    session.closedByBot = true;
    await conn.stop().catch(() => undefined);
    throw new Error(`no SocketReady within ${SOCKET_READY_TIMEOUT_MS}ms`);
  }

  session.readyAtMs = Date.now();
  session.connectionId = String(frame.connectionId ?? frame.ConnectionId ?? "");
  logEvent("socket", {
    conn: label,
    event: "ready",
    connectionId: session.connectionId,
    clientConnectionId: session.clientConnectionId,
    idsMatch: session.connectionId === session.clientConnectionId,
  });
  return session;
}

export interface DuplicateOutcome {
  label: string;
  started: boolean;
  startError?: string;
  clientConnectionId?: string;
  gotSocketReady: boolean;
  readyConnectionId?: string;
  deliveries: number;
  closed: boolean;
  closedAfterMs?: number;
  closeError?: string;
  stoppedByBot: boolean;
}

// Opens a second socket with the SAME url and access token as `primary`. APX allows one socket per token:
// its hub should abort this one before it joins the bot's group (no SocketReady) and keep the primary.
// Only call this after the primary's SocketReady; two sockets opening on one token at the same moment
// see each other and are both closed. Anything still open after `waitMs` is stopped here, so a duplicate
// APX wrongly accepted can't hold a slot of the bot's connection limit.
export async function openDuplicateSocket(
  primary: SocketSession,
  label: string,
  onActivity: (envelope: any) => Promise<unknown>,
  waitMs: number
): Promise<DuplicateOutcome> {
  const conn = buildHub(primary.negotiate, label);
  const out: DuplicateOutcome = { label, started: false, gotSocketReady: false, deliveries: 0, closed: false, stoppedByBot: false };
  let resolveSettled: () => void = () => undefined;
  const settled = new Promise<void>((resolve) => (resolveSettled = resolve));
  const startAt = Date.now();

  conn.on("SocketReady", (frame: any) => {
    out.gotSocketReady = true;
    out.readyConnectionId = String(frame?.connectionId ?? frame?.ConnectionId ?? "");
    logError("SocketReady <<", { conn: label, dir: "APX->bot", connectionId: out.readyConnectionId, note: "UNEXPECTED: APX accepted a second socket on a token another socket uses" });
    resolveSettled();
  });
  conn.on("Activity", (envelope: any) => {
    out.deliveries += 1;
    logError("route <<", { conn: label, envelopeId: envelope?.envelopeId, note: "UNEXPECTED: delivery on the duplicate-token socket; answering it" });
    return onActivity(envelope);
  });
  conn.onclose((e) => {
    out.closed = true;
    out.closedAfterMs = Date.now() - startAt;
    out.closeError = e?.message;
    logEvent("socket", {
      conn: label,
      event: "closed",
      closedBy: out.stoppedByBot ? "bot" : "server",
      closedAfterMs: out.closedAfterMs,
      error: e?.message ?? "(none)",
    });
    resolveSettled();
  });

  try {
    await conn.start();
    out.started = true;
    out.clientConnectionId = conn.connectionId ?? undefined;
    logEvent("socket", { conn: label, event: "connected (transport)", clientConnectionId: out.clientConnectionId, startMs: Date.now() - startAt });
  } catch (e) {
    out.startError = (e as Error).message;
    out.closedAfterMs = Date.now() - startAt;
    logEvent("socket", { conn: label, event: "start failed", afterMs: out.closedAfterMs, error: out.startError });
    return out;
  }

  if (!out.closed) {
    await withTimeout(settled, waitMs);
  }
  if (conn.state !== HubConnectionState.Disconnected) {
    out.stoppedByBot = true;
    logError("socket", { conn: label, event: "still open", waitedMs: waitMs, gotSocketReady: out.gotSocketReady, note: "stopping the duplicate so it can't count against the limit" });
    await conn.stop().catch(() => undefined);
  }
  return out;
}

function buildHub(neg: NegotiateResult, label: string): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(neg.url, { accessTokenFactory: () => neg.accessToken })
    .configureLogging(signalrLogger(label))
    .build();
}

function withTimeout<T>(promise: Promise<T>, ms: number): Promise<T | undefined> {
  let timer: NodeJS.Timeout | undefined;
  const timeout = new Promise<undefined>((resolve) => {
    timer = setTimeout(() => resolve(undefined), ms);
  });
  return Promise.race([promise, timeout]).finally(() => clearTimeout(timer));
}

// Routes the SignalR client's own Information+ log (transport connected, disconnect reasons, server close
// errors) into the bot's log, tagged with the connection label. The connection token (`id=` in the
// transport URL) is redacted.
function signalrLogger(label: string): ILogger {
  return {
    log(level: LogLevel, message: string): void {
      if (level < LogLevel.Information) {
        return;
      }
      const bag = { conn: label, level: LogLevel[level], msg: message.replace(/([?&]id=)[^&\s]+/g, "$1<redacted>") };
      if (level >= LogLevel.Warning) {
        logError("signalr", bag);
      } else {
        logEvent("signalr", bag);
      }
    },
  };
}
