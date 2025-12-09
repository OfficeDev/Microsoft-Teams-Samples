const config = {
  MicrosoftAppId: process.env.CLIENT_ID,
  MicrosoftAppType: process.env.BOT_TYPE,
  MicrosoftAppTenantId: process.env.TENANT_ID,
  MicrosoftAppPassword: process.env.CLIENT_PASSWORD,
  AppBaseUrl: process.env.AppBaseUrl || `https://${process.env.BOT_DOMAIN}`,
  GraphApiEndpoint: process.env.GraphApiEndpoint || 'https://graph.microsoft.com/v1.0',
  UserId: process.env.UserId,
};

module.exports = config;
