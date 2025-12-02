const config = {
  MicrosoftAppId: process.env.CLIENT_ID,
  MicrosoftAppType: process.env.BOT_TYPE,
  MicrosoftAppTenantId: process.env.TENANT_ID,
  MicrosoftAppPassword: process.env.CLIENT_PASSWORD,
  RedditClientId: process.env.REDDIT_ID,
  RedditClientSecret: process.env.SECRET_REDDIT_PASSWORD,
  Port: process.env.PORT || process.env.port || 3978,
};

module.exports = config;
