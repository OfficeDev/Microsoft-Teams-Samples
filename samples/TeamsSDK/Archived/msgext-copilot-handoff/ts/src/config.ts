const config = {
  botId: process.env.BOT_ID,
  botPassword: process.env.BOT_PASSWORD,
  botTenantId: process.env.BOT_TENANT_ID || process.env.TEAMS_APP_TENANT_ID,
  storageAccountConnectionString: process.env.STORAGE_ACCOUNT_CONNECTION_STRING
};

export default config;
