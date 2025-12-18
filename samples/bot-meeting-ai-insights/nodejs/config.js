const path = require('path');
const fs = require('fs');

// Load environment variables from env/.env.local and env/.env.local.user
const envPath = path.resolve(__dirname, 'env', '.env.local');
const envUserPath = path.resolve(__dirname, 'env', '.env.local.user');

if (fs.existsSync(envPath)) {
  require('dotenv').config({ path: envPath });
}

if (fs.existsSync(envUserPath)) {
  require('dotenv').config({ path: envUserPath });
}

const config = {
  MicrosoftAppId: process.env.CLIENT_ID || process.env.BOT_ID,
  MicrosoftAppType: process.env.BOT_TYPE || 'MultiTenant',
  MicrosoftAppTenantId: process.env.TENANT_ID || process.env.AAD_APP_TENANT_ID || process.env.TEAMS_APP_TENANT_ID,
  MicrosoftAppPassword: process.env.CLIENT_SECRET || process.env.SECRET_BOT_PASSWORD,
  connectionName: process.env.CONNECTION_NAME || 'graph',
  userId: process.env.userId,
  meetingJoinUrl: process.env.meetingJoinUrl,
  Port: process.env.PORT || process.env.port || 3978
};

module.exports = config;
