// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const path = require('path');

// Note: Ensure you have a .env file and include the MicrosoftAppId and MicrosoftAppPassword.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

const express = require('express');
const cors = require('cors');
const { CloudAdapter, ConfigurationBotFrameworkAuthentication } = require('botbuilder');

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);
const { MeetingAttendanceBot } = require('./bots/meeting-attendance-bot');

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Catch-all for errors.
adapter.onTurnError = async (context, error) => {
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

// Create the bot instance.
const bot = new MeetingAttendanceBot();

// Create HTTP server.
const server = express();
server.use(express.json());
server.use(cors());
server.use(express.urlencoded({
    extended: true
}));

server.listen(process.env.PORT || 3978, function () {
    console.log(`Server listening on http://localhost:${process.env.PORT}`);
});


// Listen for incoming activities and route them to your bot handler.
server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, (context) => bot.run(context));
});