const config = {
  MicrosoftAppId: process.env.CLIENT_ID,
  MicrosoftAppType: process.env.BOT_TYPE,
  MicrosoftAppTenantId: process.env.TENANT_ID,
  MicrosoftAppPassword: process.env.CLIENT_SECRET,
  openAIKey: process.env.OPENAI_API_KEY,
  openAIModelName: process.env.CHAT_COMPLETION_MODEL_NAME || "gpt-3.5-turbo",
  BASE_URL: process.env.BASE_URL || "http://localhost:3978"
};

module.exports = config;
