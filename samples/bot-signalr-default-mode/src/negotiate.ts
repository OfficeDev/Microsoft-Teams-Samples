import { config } from "./config";
import { getBotFrameworkToken } from "./auth";
import { logEvent, logError, cvOf, diagHeaders } from "./log";

export interface NegotiateResult {
  url: string;
  accessToken: string;
  expiresIn: number; // seconds
  protocolVersion?: number;
  // Set by the bot (not part of the APX response): when the connect returned, and the token's `exp`
  // claim (falls back to negotiatedAtMs + expiresIn when the token can't be decoded).
  negotiatedAtMs: number;
  tokenExpiresAtMs: number;
  // The token's own id (`tokenId` claim): APX allows one socket per token.
  tokenId?: string;
  // The `replaces` claim: set by APX only when the replaces header named a socket still in the bot's group.
  replacesClaim?: string;
}

// Header a refreshing client sends with the server connectionId of the socket it is about to retire.
export const REPLACES_HEADER = "x-ms-socket-replaces-connection-id";

// A non-2xx connect response. 429 = the bot is at APX's per-bot socket limit (Retry-After in seconds).
export class NegotiateError extends Error {
  constructor(
    readonly status: number,
    readonly retryAfterSec: number | undefined,
    readonly body: string,
    readonly cv: string | undefined,
    message: string
  ) {
    super(message);
  }
}

// The public APX host. Its per-region hosts ({region}.botapi.skype.com) always reach that region's front end,
// wherever the bot runs, while botapi.skype.com itself goes to the front end nearest the bot.
const PUBLIC_APX_HOST = "botapi.skype.com";
const REGIONS_WITH_OWN_HOST = new Set(["amer", "emea", "apac", "eudb"]);

// The URL the connect call goes to. On the public host, each region connects through its own host so its socket lands
// on the SignalR resource that delivers that region's traffic. Any other host (Canary, Pilot1, a local APX) is used as
// is. Only the connect call goes through here; the bot's other calls to APX (replies) use the activity's serviceUrl.
export function connectUrlFor(connectPath: string, region: string): URL {
  const url = new URL(connectPath);
  const normalizedRegion = region.toLowerCase();
  if (config.geoConnectHost && url.hostname.toLowerCase() === PUBLIC_APX_HOST && REGIONS_WITH_OWN_HOST.has(normalizedRegion)) {
    url.hostname = `${normalizedRegion}.${PUBLIC_APX_HOST}`;
  }
  return url;
}

// POST {apxBaseUrl}/{region}/v3/websockets/connect, on {region}.botapi.skype.com for the public host (connectUrlFor).
// Authenticated with a Bot Framework JWT when BOT_PASSWORD is set; APX derives the botKey from the validated
// token. A DEBUG local APX honors ?botKey=<MsaAppId> when the request is unauthenticated.
export async function negotiate(label: string | undefined, region: string, replacesConnectionId?: string): Promise<NegotiateResult> {
  const base = `${config.apxBaseUrl.replace(/\/+$/, "")}/${region}`;

  // The plain route {region}/v3/websockets/connect is the default. TENANT_IN_PATH=true forces the tenantized
  // route {region}/{tenantId}/v3/websockets/connect (needs TenantIdInPathRoutesEnabled in the ring); a local
  // DEBUG ?botKey= run always keeps the plain route.
  const useTenantPath =
    config.tenantInPath && !!config.botPassword && !!config.botTenantId;
  const connectPath = useTenantPath
    ? `${base}/${config.botTenantId}/v3/websockets/connect`
    : `${base}/v3/websockets/connect`;
  const url = connectUrlFor(connectPath, region);
  logEvent("negotiate >>", {
    conn: label,
    region,
    dir: "bot->APX",
    method: "POST",
    host: url.host,
    route: url.pathname,
    tenantized: useTenantPath,
    tenant: useTenantPath ? config.botTenantId : undefined,
    botKey: config.botKey,
    replacesHeader: replacesConnectionId ? `${REPLACES_HEADER}: ${replacesConnectionId}` : "(none)",
  });
  const headers: Record<string, string> = { "content-length": "0" };
  if (replacesConnectionId) {
    headers[REPLACES_HEADER] = replacesConnectionId;
  }

  if (config.botPassword) {
    const token = await getBotFrameworkToken();
    headers["authorization"] = `Bearer ${token}`;
  } else if (config.botKey) {
    url.searchParams.set("botKey", config.botKey);
  }

  const startedAtMs = Date.now();
  const res = await fetch(url.toString(), {
    method: "POST",
    headers,
  });
  const negotiatedAtMs = Date.now();
  const elapsedMs = negotiatedAtMs - startedAtMs;

  if (!res.ok) {
    const body = await res.text().catch(() => "");
    const diag = diagHeaders(res.headers);
    const cv = cvOf(res.headers);
    const retryAfterHeader = res.headers.get("retry-after");
    const retryAfterSec = retryAfterHeader && /^\d+$/.test(retryAfterHeader.trim()) ? Number(retryAfterHeader) : undefined;
    // MS-CV + timestamp let you correlate this failure with the APX server logs. APX logs the exact
    // eligibility reason ("WebSocket connect denied botKey=... reason=BotNotAllowed|Disabled|
    // NotConfigured|MissingBotKey") under this same cv. An empty-body 503 is the net472 connect stub
    // (WebSocketConnectController.NETFramework.cs) — the load balancer hit a non-net8 BFE instance.
    logError(
      "negotiate <<",
      { conn: label, region, dir: "APX->bot", result: "FAIL", status: res.status, retryAfter: retryAfterHeader ?? undefined, elapsedMs, diag, body: body || "(empty)" },
      cv
    );
    throw new NegotiateError(res.status, retryAfterSec, body, cv, `negotiate failed: HTTP ${res.status} ${diag} ${body}`);
  }

  const json = (await res.json()) as Omit<NegotiateResult, "negotiatedAtMs" | "tokenExpiresAtMs">;
  if (!json.url || !json.accessToken) {
    throw new Error("negotiate response missing url/accessToken");
  }

  // The Azure SignalR client token itself is never logged; only its length and its timing/identity claims,
  // so the logged exp can be checked against expiresIn and against when the socket actually closes.
  const claims = decodeJwtClaims(json.accessToken);
  const exp = typeof claims?.exp === "number" ? claims.exp : undefined;
  const iat = typeof claims?.iat === "number" ? claims.iat : undefined;
  const nbf = typeof claims?.nbf === "number" ? claims.nbf : undefined;
  const tokenExpiresAtMs = exp !== undefined ? exp * 1000 : negotiatedAtMs + json.expiresIn * 1000;
  logEvent(
    "negotiate <<",
    {
      conn: label,
      region,
      dir: "APX->bot",
      result: "OK",
      status: res.status,
      elapsedMs,
      expiresIn: json.expiresIn,
      protocolVersion: json.protocolVersion,
      signalrUrl: redactUrl(json.url),
      tokenLen: json.accessToken.length,
      tokenIat: isoOfEpochSec(iat),
      tokenNbf: isoOfEpochSec(nbf),
      tokenExp: isoOfEpochSec(exp),
      tokenLifetimeSec: exp !== undefined && iat !== undefined ? exp - iat : undefined,
      tokenAud: claims?.aud,
      tokenUserId: claims?.nameid ?? claims?.sub,
      tokenClaimNames: claims ? Object.keys(claims).join(",") : "(undecodable)",
      tokenId: claims?.tokenId,
      tokenReplacesClaim: replacesConnectionId ? claims?.replaces ?? "(absent: APX did not find that socket in the bot's group)" : undefined,
    },
    cvOf(res.headers)
  );
  return { ...json, negotiatedAtMs, tokenExpiresAtMs, tokenId: claims?.tokenId, replacesClaim: claims?.replaces };
}

function decodeJwtClaims(token: string): Record<string, any> | undefined {
  try {
    const payload = token.split(".")[1];
    return payload ? JSON.parse(Buffer.from(payload, "base64url").toString("utf8")) : undefined;
  } catch {
    return undefined;
  }
}

function isoOfEpochSec(sec: number | undefined): string | undefined {
  return sec === undefined ? undefined : new Date(sec * 1000).toISOString();
}

function redactUrl(raw: string): string {
  try {
    const u = new URL(raw);
    if (u.searchParams.has("access_token")) {
      u.searchParams.set("access_token", "<redacted>");
    }
    return u.toString();
  } catch {
    return "(unparseable url)";
  }
}
