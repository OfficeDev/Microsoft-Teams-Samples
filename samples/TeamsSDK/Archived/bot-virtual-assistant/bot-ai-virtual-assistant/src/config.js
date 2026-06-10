const config = {
  botId: process.env.BOT_ID,
  botPassword: process.env.BOT_PASSWORD,
  azureOpenAIKey: process.env.SECRET_AZURE_OPENAI_API_KEY,
  azureOpenAIEndpoint: process.env.AZURE_OPENAI_ENDPOINT,
  azureOpenAIDeploymentName: process.env.AZURE_OPENAI_DEPLOYMENT_NAME,
  MicrosoftAppType: process.env.MicrosoftAppType,
  tenantId: process.env.MicrosoftAppTenantId,
  SkillId: process.env.SkillId,
  SkillAppId: process.env.SkillAppId,
  SkillEndpoint: process.env.SkillEndpoint,
  SkillHostEndpoint: process.env.SkillHostEndpoint,
  APPINSIGHTS_INSTRUMENTATIONKEY: process.env.APPINSIGHTS_INSTRUMENTATIONKEY,
  APPINSIGHTS_CONNECTIONSTRING: process.env.APPINSIGHTS_CONNECTIONSTRING
};

module.exports = config;
