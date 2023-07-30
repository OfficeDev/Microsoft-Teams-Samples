const config = {
  botId: process.env.BOT_ID,
  botPassword: process.env.BOT_PASSWORD,
  openAIKey: process.env.SECRET_OPENAI_API_KEY,
  azureOpenApiKey: process.env.SECRET_AZURE_OPENAPI_KEY,
  completionModelUrl: process.env.COMPLETION_MODEL_URL,
  redisConnection: process.env.REDIS_CONNECTION,
  embeddingModelUrl: process.env.EMBEDDING_MODEL_URL,
  chatCompletionModelName: process.env.CHAT_COMPLETION_MODEL_NAME,
  azureStorageConnStr:process.env.AZURE_STORAGE_CONNECTION_STRING
};

export default config;
