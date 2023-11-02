// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// index.js is used to setup and configure your bot

// Import required pckages
const path = require('path');
const express = require('express');
const cors = require('cors');
const { CloudAdapter, MemoryStorage,ConfigurationBotFrameworkAuthentication,TeamsSSOTokenExchangeMiddleware } = require('botbuilder');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

const { env } = require('process');
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);
var conname = env.connectionName;
console.log(`\n${conname} is the con name`);

// Create adapter.
// See https://learn.microsoft.com/javascript/api/botbuilder-core/botadapter?view=botbuilder-ts-latest to learn more about how bot adapter.
const adapter = new CloudAdapter(botFrameworkAuthentication);
const memoryStorage = new MemoryStorage();
const tokenExchangeMiddleware = new TeamsSSOTokenExchangeMiddleware(memoryStorage, env.connectionName);

adapter.use(tokenExchangeMiddleware);

adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you must consider logging this to Azure
    //       application insights. See https://learn.microsoft.com/azure/bot-service/bot-builder-telemetry?view=azure-bot-service-4.0&tabs=csharp for telemetry
    //       configuration instructions.
    console.error(`\n [onTurnError] unhandled error: ${error}`);

    // Send a trace activity, which is displayed in Bot Framework Emulator.
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // Send a message to the user.
    await context.sendActivity('The bot encountered an error or bug.');
    await context.sendActivity('To continue to run this bot, please fix the bot source code.');
    // Clear out state.
    await conversationState.delete(context);
};

const PORT = process.env.PORT || 4001;
// Create HTTP server.
const server = express();
server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));
server.use(express.static(path.resolve(__dirname, './client/build')));

// Listen for incoming requests.
server.use('/api', require('./server/api'));

server.get('*', (req, res) => {
    res.sendFile(path.resolve(__dirname, './client/build', 'index.html'));
});

server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});
