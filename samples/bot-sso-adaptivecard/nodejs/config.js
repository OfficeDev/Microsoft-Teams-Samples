const config = {
  MicrosoftAppId: process.env.CLIENT_ID,
  MicrosoftAppType: process.env.BOT_TYPE,
  MicrosoftAppTenantId: process.env.TENANT_ID,
  MicrosoftAppPassword: process.env.CLIENT_PASSWORD,
  ConnectionName: process.env.ConnectionName,
  Port: process.env.PORT || process.env.port || 3978,
};

module.exports = config;
