// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const path = require('path');

// Note: Ensure you have a .env file and include the MicrosoftAppId and MicrosoftAppPassword.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

const express = require('express');

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder')

// This bot's main dialog.
const { ProactiveBot } = require('./bots/proactiveBot');

const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new CloudAdapter(botFrameworkAuthentication);
// Catch-all for errors.
adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights. See https://aka.ms/bottelemetry for telemetry 
    //       configuration instructions.
    //console.log(context);
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

// Create the main dialog.
const conversationReferences = {};
const bot = new ProactiveBot(conversationReferences);

// Create HTTP server.
const server = express();
const port = process.env.port || process.env.PORT || 3978;
server.listen(port, () =>
    console.log(`\Bot/ME service listening at http://localhost:${port}`)
);
server.use("/Images", express.static(path.resolve(__dirname, 'Images')));
server.use(express.json());

// Listen for incoming activities and route them to your bot main dialog.
server.post('/api/messages', async (req, res) => {
    // Route received a request to adapter for processing
    await adapter.process(req, res, (context) => bot.run(context));
});

// Listen for incoming notifications and send proactive messages to users.
server.get('/api/notify', async (req, res) => {
    for (const conversationReference of Object.values(conversationReferences)) {
        await adapter.continueConversation(conversationReference, async turnContext => {
            await turnContext.sendActivity('proactive hello');
        });
    }

    res.setHeader('Content-Type', 'text/html');
    res.writeHead(200);
    res.write('<html><body><h1>Proactive messages have been sent.</h1></body></html>');
    res.end();
});
