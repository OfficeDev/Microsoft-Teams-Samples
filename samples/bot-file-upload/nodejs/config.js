const config = {
  MicrosoftAppId: process.env.CLIENT_ID,
  MicrosoftAppType: process.env.BOT_TYPE || 'MultiTenant',
  MicrosoftAppTenantId: process.env.TENANT_ID || process.env.TEAMS_APP_TENANT_ID,
  MicrosoftAppPassword: process.env.CLIENT_SECRET || process.env.CLIENT_PASSWORD || process.env.SECRET_BOT_PASSWORD,
};



module.exports = config;
