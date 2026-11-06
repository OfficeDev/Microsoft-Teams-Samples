import { config } from "./config";
import { buildInvokeResponse } from "./invokeResponses";
import { sendActivityHttp } from "./sendActivity";
import { invokeDemoCard } from "./cards";
import { cleanUserText, getConversationScope, isBotMentioned, isGroupScope } from "./mentions";
import { logEvent, logError, truncate } from "./log";

// Must match APX Library/Services/SocketProtocol.CurrentVersion. APX's SocketInvokeDispatcher rejects
// any reply whose protocolVersion != this as a ProtocolMismatch (-> HTTP fallback), so every frame the
// bot returns MUST carry it.
const PROTOCOL_VERSION = 1;

// The reply frame the bot RETURNS from its "Activity" handler (SignalR client results). Mirrors APX's
// InvokeReplyFrame: for an invoke it carries { status, body }; for a one-way activity it is a minimal
// delivery ack (status 200, no body). protocolVersion/envelopeId/botKey are validated by APX;
// ts/recvAt are echoed for latency telemetry.
export interface ReplyFrame {
  protocolVersion: number;
  envelopeId?: string;
  status: number;
  body?: unknown;
  botKey?: string;
  ts?: number;
  recvAt?: number;
}

type ReplyBase = Pick<ReplyFrame, "protocolVersion" | "envelopeId" | "botKey" | "recvAt">;

// APX serializes the envelope with the Newtonsoft hub protocol (camelCase); read case-insensitively to
// stay robust to any casing change.
function g(obj: any, name: string): any {
  if (obj == null) return undefined;
  if (obj[name] !== undefined) return obj[name];
  const cap = name.charAt(0).toUpperCase() + name.slice(1);
  return obj[cap];
}

// Handles one inbound envelope on the "Activity" client method and RETURNS the reply frame (client
// results). APX sends two envelope kinds (SocketActivityEnvelope.Type / AckRequired):
//   * invoke   (type="invoke")    -> return the full Bot Framework invoke response over the socket.
//   * activity (ackRequired=true) -> return a minimal delivery ack (200); any content REPLY the bot
//                                    sends goes over the HTTP conversation API (sendActivity.ts), since
//                                    the socket-mode design keeps outbound activity sends on HTTP.
// Directive-driven via payload.value.directive so one bot drives every scenario (ok|error|delay|drop).
export async function handleActivity(env: any): Promise<ReplyFrame | undefined> {
  const recvAt = Date.now();
  const type = String(g(env, "type") ?? "").toLowerCase();
  const ackRequired = Boolean(g(env, "ackRequired"));
  const payload = g(env, "payload") || {};
  const envelopeId = g(env, "envelopeId");
  const cv = g(env, "cv");
  const name = g(payload, "name");
  const value = g(payload, "value") || {};
  const directive = String(value.directive ?? value.Directive ?? config.defaultDirective).toLowerCase();
  const base: ReplyBase = { protocolVersion: PROTOCOL_VERSION, envelopeId, botKey: config.botKey, recvAt };

  const isInvoke = type === "invoke" && !ackRequired;
  const scope = getConversationScope(payload);
  logEvent(
    "recv <<",
    {
      dir: "APX->bot",
      envelopeId,
      kind: isInvoke ? "invoke" : "activity",
      type,
      ackRequired,
      name,
      directive,
      scope,
      botMentioned: isBotMentioned(payload),
      payload: truncate(JSON.stringify(payload), 1500),
    },
    cv
  );

  if (!isInvoke) {
    return handleOneWayActivity(base, payload, directive, envelopeId, cv);
  }

  // ---- invoke: respond over the socket (client result) ----
  const reply = buildInvokeResponse(name, value);
  switch (directive) {
    case "drop":
      logError("reply >>", { dir: "bot->APX", envelopeId, name, action: "DROP (never returning)", note: "APX deadline -> HTTP fallback" }, cv);
      return new Promise<ReplyFrame>(() => { /* intentionally never resolves */ });

    case "delay": {
      const ms = Number(value.delayMs ?? value.DelayMs ?? 27000);
      logEvent("reply >>", { dir: "bot->APX", envelopeId, name, action: `DELAY ${ms}ms`, note: "tests the APX invoke deadline" }, cv);
      await sleep(ms);
      return reply200(base, reply, cv);
    }

    case "error":
      logError("reply >>", { dir: "bot->APX", envelopeId, name, status: 500, note: "directive=error" }, cv);
      return { ...base, status: 500, body: { error: "bot handler error (test directive=error)" }, ts: Date.now() };

    case "ok":
    default:
      return reply200(base, reply, cv);
  }
}

// One-way (non-invoke) activity: the socket-mode equivalent of onMessageActivity. The bot acks delivery
// over the socket (200), and for a message it also sends a content reply over the HTTP conversation API
// — the send-activity path the socket-mode design keeps on HTTP. Directives: ok -> ack; drop/error ->
// no ack (APX awaits its deadline -> HTTP fallback); delay -> ack after a short delay.
async function handleOneWayActivity(
  base: ReplyBase,
  payload: any,
  directive: string,
  envelopeId: string | undefined,
  cv: string | undefined
): Promise<ReplyFrame> {
  const type = String(g(payload, "type") ?? "").toLowerCase();

  if (directive === "drop" || directive === "error") {
    logError("ack >>", { dir: "bot->APX", envelopeId, type, action: `${directive} -> not acking`, note: "APX awaits deadline -> HTTP fallback" }, cv);
    return new Promise<ReplyFrame>(() => { /* intentionally never resolves */ });
  }

  if (directive === "delay") {
    await sleep(2000);
  }

  // Outbound content reply goes over HTTP (never the socket), per the socket-mode design.
  try {
    await maybeSendHttpReply(payload, cv);
  } catch (e) {
    logError("http-send <<", { dir: "APX->bot", result: "FAIL", note: "send-activity reply failed", error: (e as Error).message }, cv);
  }

  logEvent("ack >>", { dir: "bot->APX", envelopeId, type, status: 200, note: "one-way delivery confirmed" }, cv);
  return { ...base, status: 200, ts: Date.now() };
}

// Sends a content reply to a received message over the HTTP conversation API
// (POST {serviceUrl}/v3/conversations/{id}/activities) — never over the socket.
async function maybeSendHttpReply(payload: any, cv: string | undefined): Promise<void> {
  const type = String(g(payload, "type") ?? "").toLowerCase();
  if (type !== "message") {
    return; // only message activities get a content reply
  }

  const scope = getConversationScope(payload);
  const mentioned = isBotMentioned(payload);

  // In a channel or group chat Teams only routes a message to the bot when it is @-mentioned, so a
  // non-mention arriving here means the message was broadcast to the bot some other way (e.g. RSC).
  // Replying to those would make the bot answer unrelated chatter, so only mentions get a reply.
  if (isGroupScope(scope) && !mentioned) {
    logEvent("reply-card >>", { dir: "bot->APX", result: "SKIPPED", scope, note: "group/channel message without a bot @mention" }, cv);
    return;
  }

  const serviceUrl = g(payload, "serviceUrl");
  const conversation = g(payload, "conversation") || {};
  const conversationId = g(conversation, "id");
  if (!serviceUrl || !conversationId) {
    logError("http-send >>", { dir: "bot->APX", result: "SKIPPED", note: "activity missing serviceUrl/conversation.id" }, cv);
    return;
  }

  // Reply to ANY user message (not a specific keyword): echo the exact text as "You said: <text>" and
  // attach the invoke-demo card to the SAME activity. The card's Action.Execute/Action.Submit buttons
  // each fire an invoke back to the bot OVER THE SOCKET, bootstrapping the full round-trip demo.
  // In group scopes the text still carries the bot's own "<at>..</at>" tag, so strip it first — the echo
  // should show what the user typed, not the mention markup.
  const said = cleanUserText(payload);
  const reply = {
    type: "message",
    text: said ? `You said: ${said}` : `Got your ${scope} message over the socket.`,
    from: g(payload, "recipient"),
    recipient: g(payload, "from"),
    conversation,
    // In a channel the conversation id carries ";messageid=", so replying to it keeps the bot's answer in
    // the same thread rather than starting a new top-level post.
    replyToId: g(payload, "id"),
    attachments: [{ contentType: "application/vnd.microsoft.card.adaptive", content: invokeDemoCard() }],
  };

  logEvent("reply-card >>", { dir: "bot->APX", transport: "HTTP", note: "echo + invoke-demo card reply", scope, botMentioned: mentioned, conv: conversationId, userText: said.slice(0, 200) }, cv);
  const result = await sendActivityHttp(serviceUrl, conversationId, reply);
  logEvent("reply-card <<", { dir: "APX->bot", transport: "HTTP", result: "OK", status: result.status }, result.cv);
}

function reply200(base: ReplyBase, reply: { status: number; body?: unknown }, cv: string | undefined): ReplyFrame {
  const frame: ReplyFrame = { ...base, status: reply.status, body: reply.body, ts: Date.now() };
  logEvent(
    "reply >>",
    { dir: "bot->APX", transport: "socket (client result)", envelopeId: base.envelopeId, status: frame.status, body: truncate(JSON.stringify(frame.body), 1200) },
    cv
  );
  return frame;
}

function sleep(ms: number): Promise<void> {
  return new Promise((r) => setTimeout(r, ms));
}
