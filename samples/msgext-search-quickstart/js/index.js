// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your agent

// Import required packages
const path = require('path');
const express = require('express');

// Import required agent services.
// See https://aka.ms/agents-hosting to learn more about the Agent SDK.
const {
    CloudAdapter,
    loadAuthConfigFromEnv
} = require('@microsoft/agents-hosting');

// Import agent application
const { BotActivityHandler } = require('./botActivityHandler');

// Read environment variables from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Load authentication configuration from environment variables
const authConfig = loadAuthConfigFromEnv();

// Create adapter.
// See https://aka.ms/agents-hosting to learn more about adapters.
const adapter = new CloudAdapter(authConfig);

adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights.
    console.error(`\n [onTurnError] unhandled error: ${ error }`);

    // Send a trace activity, which will be displayed in Bot Framework Emulator
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${ error }`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // Only send error messages for regular conversations, not for invokes (messaging extensions)
    if (context.activity.type !== 'invoke') {
        await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);
    }
};

// Create bot handlers
const botActivityHandler = new BotActivityHandler();

// Create HTTP server.
const server = express();
server.use(express.json());
const port = process.env.port || process.env.PORT || 3978;
server.listen(port, () => 
    console.log(`Service listening at http://localhost:${port}`)
);

// Listen for incoming requests.
server.post('/api/messages', async (req, res) => {
    // Route received a request to adapter for processing
    await adapter.process(req, res, (context) => botActivityHandler.run(context));
});
