const config = {
  MicrosoftAppId: process.env.CLIENT_ID,
  MicrosoftAppType: process.env.BOT_TYPE || "SingleTenant",
  MicrosoftAppTenantId: process.env.TEAMS_APP_TENANT_ID,
  MicrosoftAppPassword: process.env.CLIENT_SECRET,
};

module.exports = config;
