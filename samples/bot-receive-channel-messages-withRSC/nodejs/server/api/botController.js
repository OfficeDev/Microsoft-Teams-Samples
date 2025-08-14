// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { BotFrameworkAdapter } = require('botbuilder');
const { BotActivityHandler } = require('../bot/botActivityHandler');

const adapter = new BotFrameworkAdapter({
    appId: process.env.MicrosoftAppId,
    appPassword: process.env.MicrosoftAppPassword
});

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
const botHandler = (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        // Process bot activity
        await botActivityHandler.run(context);
    });
};

module.exports = botHandler;