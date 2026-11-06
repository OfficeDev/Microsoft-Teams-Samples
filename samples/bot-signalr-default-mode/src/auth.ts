import { config } from "./config";

interface TokenResponse {
  access_token: string;
  expires_in: number;
  token_type: string;
}

let cached: { token: string; expiresAtMs: number } | undefined;

// Acquires (and caches) a Bot Framework connector token via the OAuth2 client-credentials flow,
// using the bot's client secret. The resulting token's audience is https://api.botframework.com and
// its issuer is the bot's home tenant — exactly what APX's OpenIdJwtAuthenticator validates for a
// bot on the authenticated POST /v3/websockets/connect endpoint (ToChannelTokenValidationParameters
// + retrieveBotTenant). Cached until ~1 minute before expiry so re-negotiates reuse it.
export async function getBotFrameworkToken(): Promise<string> {
  const now = Date.now();
  if (cached && cached.expiresAtMs - 60_000 > now) {
    return cached.token;
  }

  if (!config.botKey) {
    throw new Error("BOT_ID/BOT_KEY (bot MSA AppId) is required to mint a Bot Framework token.");
  }
  if (!config.botPassword) {
    throw new Error(
      "BOT_PASSWORD (client secret) is required for the authenticated connect. It is provisioned as " +
        "SECRET_BOT_PASSWORD and mapped to BOT_PASSWORD in .localConfigs. Point APX_BASE_URL at a local " +
        "DEBUG APX to use the unauthenticated ?botKey= override instead."
    );
  }

  const body = new URLSearchParams({
    grant_type: "client_credentials",
    client_id: config.botKey,
    client_secret: config.botPassword,
    scope: config.oauthScope,
  });

  const res = await fetch(config.oauthTokenUrl, {
    method: "POST",
    headers: { "content-type": "application/x-www-form-urlencoded" },
    body: body.toString(),
  });

  if (!res.ok) {
    const errText = await res.text().catch(() => "");
    throw new Error(`Bot Framework token request failed: HTTP ${res.status} ${errText}`);
  }

  const json = (await res.json()) as TokenResponse;
  if (!json.access_token) {
    throw new Error("Bot Framework token response missing access_token.");
  }

  const lifetimeSec = json.expires_in > 0 ? json.expires_in : 3600;
  cached = { token: json.access_token, expiresAtMs: now + lifetimeSec * 1000 };
  return json.access_token;
}
