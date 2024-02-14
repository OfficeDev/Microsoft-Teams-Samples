// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required pckages
const path = require('path');

const restify = require('restify');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname,'.env');
require('dotenv').config({ path: ENV_FILE });

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { BotFrameworkAdapter } = require('botbuilder');
const { TeamsBot } = require('./bots/teamsBot');

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about how bots work.
//const adapter = new CloudAdapter(botFrameworkAuthentication);
const adapter = new BotFrameworkAdapter({
    appId: process.env.MicrosoftAppId,
    appPassword: process.env.MicrosoftAppPassword
});

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

// Create the bot that will handle incoming messages.
const bot = new TeamsBot();

// Create HTTP server.
var server = restify.createServer();

const port = process.env.port || process.env.PORT || 3978;

// Service listening on the port 3978
server.listen(port, () =>
    console.log(`\Bot/ME service listening at http://localhost:${port}`)
);

// Listen for incoming requests.
server.post('/api/messages', (req, res, next) => {
    adapter.processActivity(req, res, async (context) => {
        await bot.run(context);
        return next();
    });
});

// Serve static pages from the 'pages' folder.
server.get('/*', restify.plugins.serveStatic({
    directory: './pages'
}));



