const config = {
  botId: process.env.BOT_ID,
  botPassword: process.env.BOT_PASSWORD,
  botTenantId: process.env.BOT_TENANT_ID,
  oauthConnectionName: process.env.OAUTH_CONNECTION_NAME || "authbot",
  storageConnectionString: process.env.AZURE_TABLE_STORAGE_CONNECTION_STRING,
  tableName: process.env.AZURE_TABLE_TABLE_NAME,
  storageTableName: process.env.AZURE_TABLE_TABLE_NAME,
};

module.exports = config;