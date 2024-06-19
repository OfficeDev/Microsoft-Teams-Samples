const config = {
  botId: process.env.BOT_ID,
  botPassword: process.env.BOT_PASSWORD,
  endpoint: process.env.END_POINT,
  apiKey: process.env.API_KEY,
  deploymentId: process.env.DEPLOYMENT_ID,
  azure_Storage_Connection_String: process.env.AZURE_STORAGE_CONNECTION_STRING,
  containerName: process.env.CONTAINER_NAME
};

module.exports = config;
