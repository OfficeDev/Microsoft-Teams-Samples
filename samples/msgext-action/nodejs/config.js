// Load environment variables first
require('dotenv').config({ path: './env/.env.local' });
require('dotenv').config({ path: './env/.env.local.user' });


const config = {
  MicrosoftAppId: process.env.BOT_ID,
  MicrosoftAppType: process.env.MicrosoftAppType || "MultiTenant",
  MicrosoftAppTenantId: process.env.TEAMS_APP_TENANT_ID,
  MicrosoftAppPassword: process.env.CLIENT_SECRET || process.env.SECRET_BOT_PASSWORD,
};


module.exports = config;
