// Minimal config for the Default-mode socket bot. All values come from env (see .env.example / .localConfigs).
// The connect URL isn't configurable: the bot connects on each region's own APX host (see negotiate.ts). Only the
// regions are configurable (APX_REGIONS).

const botTenantId = process.env.BOT_TENANT_ID || process.env.AAD_APP_TENANT_ID || "";

// Regions APX delivers from. Each region delivers only its own traffic, so by default the bot connects in all of them.
const DEFAULT_REGIONS = ["amer", "emea", "apac"];
// Regions that have their own connect host ({region}.botapi.skype.com).
const SUPPORTED_REGIONS = new Set(["amer", "emea", "apac", "eudb"]);

function parseRegions(raw: string | undefined): string[] {
  const requested = (raw || DEFAULT_REGIONS.join(","))
    .split(",")
    .map((r) => r.trim().toLowerCase())
    .filter((r) => r.length > 0);
  const unsupported = requested.filter((r) => !SUPPORTED_REGIONS.has(r));
  if (unsupported.length > 0) {
    console.warn(`[config] ignoring unsupported APX_REGIONS value(s): ${unsupported.join(", ")} (supported: ${[...SUPPORTED_REGIONS].join(", ")})`);
  }
  const regions = [...new Set(requested.filter((r) => SUPPORTED_REGIONS.has(r)))];
  return regions.length > 0 ? regions : DEFAULT_REGIONS;
}

export const config = {
  // Regions to connect in, one socket each (comma-separated). Defaults to amer, emea and apac.
  regions: parseRegions(process.env.APX_REGIONS),

  // The bot's MSA AppId = the socket "botKey". APX stamps it as the hub connection's UserId so its
  // connection directory can resolve botKey -> connectionId for the client-results invoke. Teams
  // Toolkit provision fills BOT_ID (the created Entra app's client id); BOT_KEY is the fallback for a
  // manual `npm run dev`. APX derives the botKey from the validated token.
  botKey: process.env.BOT_ID || process.env.BOT_KEY || "",

  // The bot's client secret. Required for the connect call: the bot mints a Bot Framework JWT with it (see
  // auth.ts). Provisioned as SECRET_BOT_PASSWORD and mapped to BOT_PASSWORD in .localConfigs.
  botPassword: process.env.BOT_PASSWORD || process.env.SECRET_BOT_PASSWORD || "",

  // The bot's home tenant (single-tenant Entra app, signInAudience=AzureADMyOrg). This is the token
  // issuer APX validates for a bot (OpenIdJwtAuthenticator -> retrieveBotTenant).
  botTenantId,

  // Connect route selector. false (default) = the plain route {region}/v3/websockets/connect. A socket connect
  // is a bot-level operation with no conversation, so APX resolves no tenant for it. true = force the tenantized
  // route {region}/{tenantId}/v3/websockets/connect, which also requires TenantIdInPathRoutesEnabled in the ring.
  tenantInPath: (process.env.TENANT_IN_PATH || "false").toLowerCase() === "true",

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
