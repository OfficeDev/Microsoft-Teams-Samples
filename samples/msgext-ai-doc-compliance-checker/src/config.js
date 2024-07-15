const config = {
  botId: process.env.BOT_ID,
  botPassword: process.env.BOT_PASSWORD,
  endpoint: process.env.END_POINT,
  apiKey: process.env.API_KEY,
  deploymentId: process.env.DEPLOYMENT_ID,
  azure_Storage_Connection_String: process.env.AZURE_STORAGE_CONNECTION_STRING,
  azure_containerName: process.env.AZURE_CONTAINER_NAME,
  CheckListFileName: process.env.CHECKLIST_NAME
};

module.exports = config;
