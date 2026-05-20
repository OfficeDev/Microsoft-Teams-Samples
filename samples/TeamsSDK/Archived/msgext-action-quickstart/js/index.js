// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js sets up the bot's HTTP listener and adapter.

const path = require('path');
const express = require('express');

// Load environment variables from .env before importing modules that use them.
require('dotenv').config({ path: path.join(__dirname, '.env') });

const {
    CloudAdapter,
    ConfigurationServiceClientCredentialFactory,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');

const { BotActivityHandler } = require('./botActivityHandler');

// Configure bot framework authentication using values from process.env
// (MicrosoftAppId, MicrosoftAppPassword, MicrosoftAppType, MicrosoftAppTenantId).
const credentialsFactory = new ConfigurationServiceClientCredentialFactory({
    MicrosoftAppId: process.env.MicrosoftAppId,
    MicrosoftAppPassword: process.env.MicrosoftAppPassword,
    MicrosoftAppType: process.env.MicrosoftAppType,
    MicrosoftAppTenantId: process.env.MicrosoftAppTenantId
});

const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(
    {},
    credentialsFactory
);

const adapter = new CloudAdapter(botFrameworkAuthentication);

adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );
};

const botActivityHandler = new BotActivityHandler();

// Create the HTTP server.
const server = express();
server.use(express.json());

const port = process.env.port || process.env.PORT || 3978;
server.listen(port, () => {
    console.log(`\nBot/ME service listening at http://localhost:${port}`);
});

// Listen for incoming requests.
server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, (context) => botActivityHandler.run(context));
});
