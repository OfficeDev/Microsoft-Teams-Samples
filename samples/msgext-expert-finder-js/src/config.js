const config = {
  botId: process.env.BOT_ID,
  botPassword: process.env.BOT_PASSWORD,
  storageConnectionString: process.env.AZURE_TABLE_STORAGE_CONNECTION_STRING,
  storageTableName: process.env.AZURE_TABLE_TABLE_NAME,
};

module.exports = config;