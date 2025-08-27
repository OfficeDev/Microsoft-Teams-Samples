// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');

const dotenv = require('dotenv');
// Import required bot configuration.
const ENV_FILE = path.join(__dirname, '.env');

dotenv.config({ path: ENV_FILE });

const PORT = process.env.PORT || 3978;
const express = require('express');
const cors = require('cors');

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder')

const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);
// This bot's main dialog.
//const { DeepLinksTest } = require('./bot');
const DeepLinkTabsnode = require('./Bots/DeepLinkTabsnode');

// Create HTTP server
const server = express();
server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));

server.listen(PORT, () => {
    console.log('Server listening on port: ' + PORT);
});

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about how bots work.
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Catch-all for errors.
const onTurnErrorHandler = async (context, error) => {
    debugger
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights. See https://aka.ms/bottelemetry for telemetry 
    //       configuration instructions.
    console.error(`\n [onTurnError] unhandled error: ${error}`);

    // Send a trace activity, which will be displayed in Bot Framework Emulator
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // Uncomment below commented line for local debugging.
    // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);
};

// Set the onTurnError for the singleton BotFrameworkAdapter.
adapter.onTurnError = onTurnErrorHandler;

// Create the main dialog.
const myBot = new DeepLinkTabsnode();

// Listen for incoming requests.
server.post('/api/messages', async (req, res) => {
    // Route received a request to adapter for processing
    await adapter.process(req, res, (context) => myBot.run(context));
});

// Listen for Upgrade requests for Streaming.
server.on('upgrade', (req, socket, head) => {
    // Create an adapter scoped to this WebSocket connection to allow storing session data.
    const streamingAdapter = new BotFrameworkAdapter({
        appId: process.env.MicrosoftAppId,
        appPassword: process.env.MicrosoftAppPassword
    });
    // Set onTurnError for the BotFrameworkAdapter created for each connection.
    streamingAdapter.onTurnError = onTurnErrorHandler;

    streamingAdapter.useWebSocket(req, socket, head, async (context) => {
        // After connecting via WebSocket, run this logic for every request sent over
        // the WebSocket connection.
        await myBot.run(context);
    });
});

server.get('/api/getAppId', (req, res) => {
    res.send({ microsoftAppId: process.env.MicrosoftAppId });
});


// Static Middleware
server.use(express.static(path.join(__dirname, './pages')));