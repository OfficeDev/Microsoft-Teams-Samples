const config = {
  MicrosoftAppId: process.env.CLIENT_ID,
  MicrosoftAppType: process.env.BOT_TYPE,
  MicrosoftAppTenantId: process.env.TENANT_ID,
  MicrosoftAppPassword: process.env.CLIENT_SECRET,
  AzureOpenAIEndpoint: process.env.AzureOpenAIEndpoint,
  AzureOpenAIKey: process.env.AzureOpenAIKey,
  AzureOpenAIDeployment: process.env.AzureOpenAIDeployment,
};

module.exports = config;
