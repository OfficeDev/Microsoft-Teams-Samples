const config = {
  MicrosoftAppId: process.env.BOT_ID,
  MicrosoftAppType: process.env.BOT_TYPE,
  MicrosoftAppTenantId: process.env.BOT_TENANT_ID,
  MicrosoftAppPassword: process.env.BOT_PASSWORD,
  openAIKey: process.env.OPENAI_API_KEY,
  openAIModelName: "gpt-3.5-turbo",
};

module.exports = config;
