const config = {
  MicrosoftAppId: process.env.CLIENT_ID,
  MicrosoftAppType: process.env.BOT_TYPE,
  MicrosoftAppTenantId: process.env.TENANT_ID,
  MicrosoftAppPassword: process.env.CLIENT_PASSWORD,
  connectionName: process.env.CONNECTION_NAME || 'oauthbotsetting',
  appCatalogTeamAppId: process.env.APP_CATALOG_TEAM_APP_ID,
};

export default config;
