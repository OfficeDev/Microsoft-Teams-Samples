const config = {
  botId: process.env.BOT_ID,
  botPassword: process.env.BOT_PASSWORD,
  azureOpenAIKey: process.env.AZURE_OPENAI_API_KEY,
  azureOpenAIEndpoint: process.env.AZURE_OPENAI_ENDPOINT,
  azureOpenAIDeploymentName: process.env.AZURE_OPENAI_DEPLOYMENT_NAME,
  MicrosoftAppType: process.env.MicrosoftAppType,
  tenantId: process.env.MicrosoftAppTenantId,
  SkillId: "EchoSkillBot,OpenAiSkillBot",
  SkillAppId: "14e4cb50-82ce-4573-8385-2e74bf1edbd0,998691d7-1b8a-4029-a305-aa07dc2d3ee6",
  SkillEndpoint : "http://localhost:39783/api/messages,http://localhost:39784/api/messages",
  SkillHostEndpoint : "http://localhost:3978/api/skills/"
};

module.exports = config;
