// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Read environment variables from .env file
require('dotenv').config();

const config = {
  botId: process.env.BOT_ID || process.env.MicrosoftAppId || process.env.CLIENT_ID,
  botPassword: process.env.BOT_PASSWORD || process.env.MicrosoftAppPassword || process.env.CLIENT_PASSWORD,
  botType: process.env.BOT_TYPE,
  tenantId: process.env.TENANT_ID,
  baseUrl: process.env.BOT_ENDPOINT || process.env.BaseUrl || `http://localhost:${process.env.port || process.env.PORT || 3978}`,
  botDomain: process.env.BOT_DOMAIN || 'localhost:3980',
};

module.exports = config;
