const config = {
  botId: process.env.BOT_ID,
  botPassword: process.env.BOT_PASSWORD,
  azureOpenApiKey: process.env.SECRET_AZURE_OPENAPI_KEY,
  completionModelUrl: process.env.COMPLETION_MODEL_URL,
  redisConnection: process.env.REDIS_CONNECTION,
  embeddingModelUrl: process.env.EMBEDDING_MODEL_URL,
  azureStorageConnStr:process.env.AZURE_STORAGE_CONNECTION_STRING
};

export default config;
