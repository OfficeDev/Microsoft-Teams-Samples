// Minimal config for the Default-mode socket test bot. All values come from env (see .env.example
// / .localConfigs). Nothing Teams-specific here — this bot exists only to exercise APX Default-mode
// socket invokes (Azure SignalR app-hosted hub + client results). Targets APX Canary by default
// (authenticated connect); override APX_BASE_URL (and clear BOT_PASSWORD) for a local DEBUG APX.

const botTenantId = process.env.BOT_TENANT_ID || process.env.AAD_APP_TENANT_ID || "";

export const config = {
  // APX endpoint that hosts the WebSocket connect (negotiate) API. Include the cloud/region segment
  // (e.g. /amer) so the regional connect route {cloud}/v3/websockets/connect is used.
  // On Canary the public, bot-authenticated connect lives on the BotFrontEnd gateway
  // (https://canary.botapi.skype.com/amer/v3/websockets/connect); Azure SignalR then routes the live
  // socket to BotNotifications, which hosts the hub and mints/dispatches the client-results invoke.
  // (The old https://canary-notifications.botapi.skype.com:444 ingress rejects a bot bearer at SEAL.)
  // For a local DEBUG APX set APX_BASE_URL=https://localhost:444 and leave BOT_PASSWORD empty.
  apxBaseUrl: process.env.APX_BASE_URL || "https://canary.botapi.skype.com/amer",

  // The bot's MSA AppId = the socket "botKey". APX stamps it as the hub connection's UserId so its
  // connection directory can resolve botKey -> connectionId for the client-results invoke. Teams
  // Toolkit provision fills BOT_ID (the created Entra app's client id); BOT_KEY is the fallback for a
  // manual `npm run dev`. On Canary the botKey is derived server-side from the validated token; on a
  // DEBUG local APX build the negotiate endpoint accepts ?botKey=<this> without auth.
  botKey: process.env.BOT_ID || process.env.BOT_KEY || "",

  // The bot's client secret. Required for the authenticated connect against Canary (or any non-DEBUG
  // APX): the bot mints a real Bot Framework JWT with it (see auth.ts). Provisioned as
  // SECRET_BOT_PASSWORD and mapped to BOT_PASSWORD in .localConfigs. When absent, the bot falls back
  // to the DEBUG ?botKey= override (local DEBUG APX only).
  botPassword: process.env.BOT_PASSWORD || process.env.SECRET_BOT_PASSWORD || "",

  // The bot's home tenant (single-tenant Entra app, signInAudience=AzureADMyOrg). This is the token
  // issuer APX validates for a bot (OpenIdJwtAuthenticator -> retrieveBotTenant).
  botTenantId,

  // Connect via the tenantized route {cloud}/{tenantId}/v3/websockets/connect so BotFrontEnd's
  // TenantIdInPathFilter sets ctx.TenantId before the socket-eligibility check (the bot token alone
  // carries no tenant). Needed because DeliverEventViaSocketEnabled is tenant-scoped and can't match
  // on a plain connect. Set TENANT_IN_PATH=false to fall back to the plain route (e.g. if the ring
  // doesn't have TenantIdInPathRoutesEnabled on, or for a local DEBUG APX).
  tenantInPath: (process.env.TENANT_IN_PATH || "true").toLowerCase() !== "false",

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
