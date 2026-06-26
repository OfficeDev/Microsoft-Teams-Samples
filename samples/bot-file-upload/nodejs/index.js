// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const path = require('path');
const restify = require('restify');
const { CloudAdapter, ConfigurationBotFrameworkAuthentication } = require('botbuilder');
const { TeamsFileUploadBot } = require('./bots/teamsFileUploadBot');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Initialize bot authentication configuration
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

// Create adapter
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Global error handling
adapter.onTurnError = async (context, error) => {
    // Log error for debugging
    console.error(`[onTurnError] unhandled error: ${error.message}`);

    // Send a trace activity for debugging in Bot Framework Emulator
    await context.sendTraceActivity(
        'OnTurnError Trace',
        error.message,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // Uncomment below for local debugging:
    // await context.sendActivity(`Sorry, something went wrong: ${error.message}`);
};

// Create the bot that will handle incoming messages
const bot = new TeamsFileUploadBot();

// Set up the HTTP server
const server = restify.createServer();
server.use(restify.plugins.bodyParser());

// Start the server
server.listen(process.env.port || process.env.PORT || 3978, function() {
    console.log(`\n${ server.name } listening to ${ server.url }`);
});

// Listen for incoming requests and route them to the bot
server.post('/api/messages', async (req, res) => {
    // Route incoming requests to adapter for processing
    await adapter.process(req, res, async (context) => {
        await bot.run(context);
    });
});
