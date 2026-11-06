// Real Teams invoke handling — builds the appropriate Bot Framework invoke-response BODY for a given
// invoke name, mirroring prem-test-me-bot's response shapes (compose-extension result,
// AdaptiveCardInvokeResponse, task module, signin, default/unknown). The socket bot returns these as
// the client-results reply frame's { status, body }, exactly as the HTTP bot returns
// { status, JSON(body) } — so APX's InvokeHelper.ProcessInvokeResponse sees the same shape on both
// transports.
//
// adaptiveCard/action is verb-aware (invokeDemoCard's Action.Execute buttons) so each button yields a
// visibly different socket response: refreshCard updates the card in place, showMessage shows a toast,
// errorAction returns a message noting a simulated error. task/fetch returns a dialog over the socket.

import { invokeDemoCard, refreshedCard, taskModuleCard } from "./cards";

export interface InvokeReply {
  status: number;
  body?: unknown;
}

// Case-insensitive property read (APX camelCases the envelope; stay robust to any casing change).
function g(obj: any, name: string): any {
  if (obj == null) return undefined;
  if (obj[name] !== undefined) return obj[name];
  const cap = name.charAt(0).toUpperCase() + name.slice(1);
  return obj[cap];
}

// Monotonic click counter so the refreshed card visibly changes each round-trip.
let actionClicks = 0;

function heroAttachment(title: string): Record<string, unknown> {
  const content = { title, text: "socket test bot result" };
  return {
    contentType: "application/vnd.microsoft.card.hero",
    content,
    preview: { contentType: "application/vnd.microsoft.card.hero", content },
  };
}

function adaptiveAttachment(card: Record<string, unknown>): Record<string, unknown> {
  return { contentType: "application/vnd.microsoft.card.adaptive", content: card };
}

// AdaptiveCardInvokeResponse — { statusCode, type, value }. Teams renders `value` (a card) in place.
function adaptiveCardInvokeResponse(statusCode: number, card: Record<string, unknown>): Record<string, unknown> {
  return { statusCode, type: "application/vnd.microsoft.card.adaptive", value: card };
}

// A message-type invoke response — Teams shows `value` as a transient message/toast.
function messageInvokeResponse(text: string): Record<string, unknown> {
  return { statusCode: 200, type: "application/vnd.microsoft.activity.message", value: text };
}

// Handles the adaptiveCard/action invoke (Action.Execute), branching on the button's verb.
function handleAdaptiveCardAction(value: any): InvokeReply {
  const action = g(value, "action") || {};
  const verb = String(g(action, "verb") ?? "").toLowerCase();
  const nowIso = new Date().toISOString();

  switch (verb) {
    case "refreshcard": {
      actionClicks += 1;
      return { status: 200, body: adaptiveCardInvokeResponse(200, refreshedCard(actionClicks, nowIso)) };
    }
    case "showmessage":
      return { status: 200, body: messageInvokeResponse(`Handled over the socket at ${nowIso}.`) };
    case "erroraction":
      // The socket delivery/response itself succeeds (status 200); the body signals a simulated error
      // so it's clearly visible in the client. (Use the `directive=error` path to test a 500 reply.)
      return { status: 200, body: messageInvokeResponse("Simulated error response — returned over the socket.") };
    default:
      return { status: 200, body: adaptiveCardInvokeResponse(200, invokeDemoCard(`Unknown verb '${verb}' handled over the socket.`)) };
  }
}

// Maps an invoke `name` to its { status, body }. Unknown names fall through to a benign default,
// matching prem-test-me-bot's onInvokeActivity default branch.
export function buildInvokeResponse(name: string | undefined, value: any): InvokeReply {
  const n = (name || "").toLowerCase();
  switch (n) {
    case "adaptivecard/action":
      return handleAdaptiveCardAction(value);

    case "composeextension/query":
    case "composeextension/querylink":
    case "composeextension/anonymousquerylink":
      return {
        status: 200,
        body: { composeExtension: { type: "result", attachmentLayout: "list", attachments: [heroAttachment("Socket query result")] } },
      };

    case "composeextension/submitaction":
      return {
        status: 200,
        body: { composeExtension: { type: "result", attachmentLayout: "list", attachments: [adaptiveAttachment(invokeDemoCard())] } },
      };

    case "composeextension/fetchtask":
    case "task/fetch":
      return {
        status: 200,
        body: { task: { type: "continue", value: { title: "Socket task module", height: 250, width: 400, card: adaptiveAttachment(taskModuleCard()) } } },
      };

    case "task/submit":
      return { status: 200, body: { task: { type: "message", value: "Submitted over the socket — thanks!" } } };

    case "signin/verifystate":
    case "signin/tokenexchange":
      return { status: 200, body: {} };

    // 200 with no body — Bot Framework's CreateInvokeResponse(200) / { status: 200 } pattern.
    case "message/submitaction":
    case "suggestedactions/submit":
    case "voteinvoke":
      return { status: 200 };

    default:
      return { status: 200, body: `Unknown invoke activity handled as default- ${name}` };
  }
}
