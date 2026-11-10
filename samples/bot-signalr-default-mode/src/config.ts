// Minimal config for the Default-mode socket bot. All values come from env (see .env.example
// / .localConfigs). Targets the production APX connect endpoint by default (authenticated connect) and
// opens one socket per region; override APX_BASE_URL (and clear BOT_PASSWORD) for a local DEBUG APX.

const botTenantId = process.env.BOT_TENANT_ID || process.env.AAD_APP_TENANT_ID || "";

// Regions APX delivers from. Each region delivers only its own traffic, so the bot connects in all of them.
const KNOWN_REGIONS = ["amer", "emea", "apac"];

// The connect host without a region. An older-style value that still ends in a region (e.g.
// https://botapi.skype.com/amer) has the region stripped, since the region is now added per socket.
function baseUrlWithoutRegion(raw: string): string {
  const trimmed = raw.replace(/\/+$/, "");
  const last = trimmed.substring(trimmed.lastIndexOf("/") + 1).toLowerCase();
  return KNOWN_REGIONS.includes(last) ? trimmed.substring(0, trimmed.lastIndexOf("/")) : trimmed;
}

function parseRegions(raw: string | undefined): string[] {
  const regions = (raw || KNOWN_REGIONS.join(","))
    .split(",")
    .map((r) => r.trim().toLowerCase())
    .filter((r) => r.length > 0);
  return regions.length > 0 ? [...new Set(regions)] : KNOWN_REGIONS;
}

export const config = {
  // APX host that serves the WebSocket connect API, without a region. The bot calls the regional route
  // {apxBaseUrl}/{region}/v3/websockets/connect once per region. Azure SignalR then routes the live socket
  // to BotNotifications, which hosts the hub and dispatches the client-results invoke.
  // For a local DEBUG APX set APX_BASE_URL=https://localhost:444, APX_REGIONS=amer and leave BOT_PASSWORD empty.
  apxBaseUrl: baseUrlWithoutRegion(process.env.APX_BASE_URL || "https://botapi.skype.com"),

  // Regions to connect in, one socket each (comma-separated). Defaults to every region.
  regions: parseRegions(process.env.APX_REGIONS),

  // The bot's MSA AppId = the socket "botKey". APX stamps it as the hub connection's UserId so its
  // connection directory can resolve botKey -> connectionId for the client-results invoke. Teams
  // Toolkit provision fills BOT_ID (the created Entra app's client id); BOT_KEY is the fallback for a
  // manual `npm run dev`. APX derives the botKey from the validated token; on a DEBUG local APX build
  // the negotiate endpoint accepts ?botKey=<this> without auth.
  botKey: process.env.BOT_ID || process.env.BOT_KEY || "",

  // The bot's client secret. Required for the authenticated connect (any non-DEBUG
  // APX): the bot mints a real Bot Framework JWT with it (see auth.ts). Provisioned as
  // SECRET_BOT_PASSWORD and mapped to BOT_PASSWORD in .localConfigs. When absent, the bot falls back
  // to the DEBUG ?botKey= override (local DEBUG APX only).
  botPassword: process.env.BOT_PASSWORD || process.env.SECRET_BOT_PASSWORD || "",

  // The bot's home tenant (single-tenant Entra app, signInAudience=AzureADMyOrg). This is the token
  // issuer APX validates for a bot (OpenIdJwtAuthenticator -> retrieveBotTenant).
  botTenantId,

  // Connect route selector. false (default) = the plain route {region}/v3/websockets/connect. A socket connect
  // is a bot-level operation with no conversation, so APX resolves no tenant for it. true = force the tenantized
  // route {region}/{tenantId}/v3/websockets/connect, which also requires TenantIdInPathRoutesEnabled in the ring.
  tenantInPath: (process.env.TENANT_IN_PATH || "false").toLowerCase() === "true",

  // When APX_BASE_URL is the public https://botapi.skype.com, connect through each region's own host
  // ({region}.botapi.skype.com). That host always reaches the region's front end, so the socket lands on the SignalR
  // resource that delivers the region's traffic; botapi.skype.com itself goes to the front end nearest the bot, which
  // hands out its own region's resource for every region. Only the connect call uses it. Set APX_GEO_CONNECT_HOST=false
  // to connect through APX_BASE_URL as is.
  geoConnectHost: (process.env.APX_GEO_CONNECT_HOST || "true").toLowerCase() !== "false",

  // Bot Framework connector token endpoint + scope. Defaults: the bot's home-tenant STS and the
  // standard connector audience (https://api.botframework.com) — exactly what APX's
  // ToChannelTokenValidationParameters accept for a bot. Override OAUTH_TOKEN_URL for a multi-tenant
  // (MSA) bot, e.g. https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token.
  oauthTokenUrl:
    process.env.OAUTH_TOKEN_URL ||
    `https://login.microsoftonline.com/${botTenantId || "botframework.com"}/oauth2/v2.0/token`,
  oauthScope: process.env.OAUTH_SCOPE || "https://api.botframework.com/.default",

  // Behavior when an invoke carries no explicit directive in payload.value.directive.
  // One of: ok | error | delay | drop.
  defaultDirective: (process.env.DEFAULT_DIRECTIVE || "ok").toLowerCase(),
};
