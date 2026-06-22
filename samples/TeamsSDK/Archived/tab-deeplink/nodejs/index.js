// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const dotenv = require('dotenv');
const express = require('express');
const cors = require('cors');
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');

const DeepLinkTabsnode = require('./Bots/DeepLinkTabsnode');

// Load bot configuration from .env file.
const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });

const PORT = process.env.PORT || 3978;

// Create HTTP server.
const server = express();
server.use(cors());
server.use(express.json());
server.use(express.urlencoded({ extended: true }));

server.listen(PORT, () => {
    console.log('Server listening on port: ' + PORT);
});

// Create the bot framework authentication and adapter.
// See https://aka.ms/about-bot-adapter to learn more about how bots work.
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Catch-all error handler. In production, consider logging to Application Insights.
// See https://aka.ms/bottelemetry for telemetry configuration instructions.
adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);

    // Send a trace activity, which will be displayed in Bot Framework Emulator.
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );
};

// Create the bot.
const myBot = new DeepLinkTabsnode();

// Listen for incoming bot requests.
server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, (context) => myBot.run(context));
});

// Expose the Microsoft App Id to client-side pages that need it (e.g. for deep links).
server.get('/api/getAppId', (req, res) => {
    res.send({ microsoftAppId: process.env.MicrosoftAppId });
});

// Serve the static page assets.
server.use(express.static(path.join(__dirname, './pages')));