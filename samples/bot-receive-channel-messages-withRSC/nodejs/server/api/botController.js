// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const { CloudAdapter, ConfigurationBotFrameworkAuthentication } = require('botbuilder');
const { BotActivityHandler } = require('../bot/botActivityHandler');

// Load environment variables from .env file
const ENV_FILE = path.join(__dirname, '../../.env');
require('dotenv').config({ path: ENV_FILE });

// Initialize bot framework authentication using environment variables
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Handle errors during bot turn processing
adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);

    // Send a trace activity, which will be displayed in Bot Framework Emulator
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // Uncomment the line below for local debugging
    // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);
};

// Create bot handlers
const botActivityHandler = new BotActivityHandler();

// This function is the entry point for every incoming request from Teams/Emulator
const botHandler = async (req, res) => {
    console.log("Incoming request at /api/messages");
    console.log("Raw body:", JSON.stringify(req.body, null, 2)); // Debug raw payload

    await adapter.process(req, res, async (context) => {
        console.log("Processing activity:", context.activity.type, context.activity.text);
        await botActivityHandler.run(context);
        console.log("Activity processed by BotActivityHandler");
    });
};

module.exports = botHandler;
