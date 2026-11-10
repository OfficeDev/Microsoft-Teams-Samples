import {
  HubConnection,
  HubConnectionBuilder,
  IHttpConnectionOptions,
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
  // The region this socket serves (amer, emea, apac). A refresh keeps the region and changes the socket.
  region: string;
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
  retiredMode?: "header";
}

export interface SocketCallbacks {
  onActivity: (session: SocketSession, envelope: any) => Promise<unknown>;
  onClosed: (session: SocketSession, error?: Error) => void;
}

export interface OpenOptions {
  region: string;
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
// The SignalR client's automatic reconnect is off: every new socket needs a new connect call, so the
// keep-alive (keepAlive.ts) refreshes sockets before their token expires and reconnects after a drop.
export async function openSocket(
  index: number,
  label: string,
  callbacks: SocketCallbacks,
  options: OpenOptions
): Promise<SocketSession> {
  const neg = await negotiate(label, options.region, options.replacesConnectionId);
  const conn = buildHub(neg, label);

  const session: SocketSession = {
    index,
    label,
    region: options.region,
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
      region: options.region,
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
    region: options.region,
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
    region: options.region,
    event: "ready",
    connectionId: session.connectionId,
    clientConnectionId: session.clientConnectionId,
    idsMatch: session.connectionId === session.clientConnectionId,
  });
  return session;
}

function buildHub(neg: NegotiateResult, label: string): HubConnection {
  // `WebSocket` is honored by the client (HttpConnection) but missing from the public options type.
  const options: IHttpConnectionOptions & { WebSocket?: unknown } = {
    accessTokenFactory: () => neg.accessToken,
    WebSocket: closeLoggingWebSocket(label),
  };
  return new HubConnectionBuilder()
    .withUrl(neg.url, options)
    .configureLogging(signalrLogger(label))
    .build();
}

// eslint-disable-next-line @typescript-eslint/no-var-requires
const WsWebSocket: any = require("ws");

// The `ws` WebSocket the SignalR client uses in Node, extended to log the raw close code and reason the
// service sends. The SignalR client reports a normal close (1000) as a plain disconnect without them, so this
// is the only place the bot sees how Azure SignalR ended a socket (e.g. at token expiry). Only listens; adds no
// behavior.
function closeLoggingWebSocket(label: string): any {
  return class extends WsWebSocket {
    constructor(...args: any[]) {
      super(...args);
      const openedAt = Date.now();
      this.on("close", (code: number, reason: Buffer | string) => {
        const text = reason ? reason.toString() : "";
        logEvent("ws", {
          conn: label,
          event: "websocket closed",
          code,
          codeMeaning: closeCodeMeaning(code),
          reason: text || "(none)",
          openSec: ((Date.now() - openedAt) / 1000).toFixed(1),
        });
      });
    }
  };
}

function closeCodeMeaning(code: number): string {
  switch (code) {
    case 1000:
      return "normal closure";
    case 1001:
      return "going away";
    case 1005:
      return "no status code";
    case 1006:
      return "abnormal closure (no close frame)";
    case 1008:
      return "policy violation";
    case 1011:
      return "server error";
    default:
      return code >= 4000 ? "application-defined" : "other";
  }
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
