const config = {
  MicrosoftAppId: process.env.CLIENT_ID || process.env.BOT_ID,
  MicrosoftAppType: process.env.BOT_TYPE,
  MicrosoftAppTenantId: process.env.TENANT_ID,
  MicrosoftAppPassword: process.env.CLIENT_PASSWORD,
  BOT_ENDPOINT: process.env.BOT_ENDPOINT || process.env.BOT_DOMAIN ? `https://${process.env.BOT_DOMAIN}` : '',
  TEAMS_APP_ID: process.env.TEAMS_APP_ID,
  BOT_DOMAIN: process.env.BOT_DOMAIN,
  ApplicationBaseUrl: process.env.BOT_ENDPOINT || process.env.ApplicationBaseUrl || process.env.BOT_DOMAIN ? `https://${process.env.BOT_DOMAIN}` : '',
};

module.exports = config;
