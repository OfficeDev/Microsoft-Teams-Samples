const config = {
  MicrosoftAppId: process.env.BOT_ID,
  MicrosoftAppPassword: process.env.BOT_PASSWORD,
  MicrosoftAppType: process.env.MICROSOFT_APP_TYPE || "SingleTenant",
  MicrosoftAppTenantId: process.env.MICROSOFT_APP_TENANT_ID || "",
};

module.exports = config;
