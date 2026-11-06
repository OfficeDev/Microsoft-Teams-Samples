import { config } from "./config";
import { getBotFrameworkToken } from "./auth";
import { logEvent, logError, cvOf, diagHeaders } from "./log";

export interface SendResult {
  status: number;
  cv?: string;
}

// Sends an outbound activity to a conversation over the HTTP conversation API — the path the socket
// mode design keeps on HTTP. The socket carries only APX -> bot delivery and the bot's invoke/ack
// response (client results); when the bot SENDS content (a reply/proactive message) it POSTs to
// {serviceUrl}/v3/conversations/{conversationId}/activities with its Bot Framework JWT (the same
// credential used for the connect). Falls back to no auth header for a local DEBUG APX.
export async function sendActivityHttp(
  serviceUrl: string,
  conversationId: string,
  activity: unknown
): Promise<SendResult> {
  if (!serviceUrl || !conversationId) {
    throw new Error("sendActivityHttp requires serviceUrl and conversationId from the inbound activity.");
  }

  const base = serviceUrl.replace(/\/+$/, "");
  const url = `${base}/v3/conversations/${encodeURIComponent(conversationId)}/activities`;

  const headers: Record<string, string> = { "content-type": "application/json" };
  if (config.botPassword) {
    headers["authorization"] = `Bearer ${await getBotFrameworkToken()}`;
  }

  const activityType = (activity as any)?.type;
  logEvent("http-send >>", { dir: "bot->APX", method: "POST", url, conv: conversationId, activityType });

  let res: Response;
  try {
    res = await fetch(url, { method: "POST", headers, body: JSON.stringify(activity) });
  } catch (e) {
    // Network/transport failure — no response, so no MS-CV to correlate.
    logError("http-send <<", { dir: "APX->bot", result: "TRANSPORT_ERROR", url, conv: conversationId, error: (e as Error).message });
    throw e;
  }

  const cv = cvOf(res.headers);
  if (!res.ok) {
    const errText = await res.text().catch(() => "");
    logError(
      "http-send <<",
      { dir: "APX->bot", result: "FAIL", status: res.status, url, conv: conversationId, diag: diagHeaders(res.headers), body: errText || "(empty)" },
      cv
    );
    throw new Error(`HTTP sendActivity failed: HTTP ${res.status} ${diagHeaders(res.headers)} ${errText}`);
  }

  logEvent("http-send <<", { dir: "APX->bot", result: "OK", status: res.status, url, conv: conversationId }, cv);
  return { status: res.status, cv };
}
