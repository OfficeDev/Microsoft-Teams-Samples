// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const path = require('path');
// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

const restify = require('restify');


// Import required bot services
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { CloudAdapter, ConfigurationBotFrameworkAuthentication } = require('botbuilder');
const { TeamsStartNewThreadInChannel } = require('./bots/TeamsStartNewThreadInChannel');

// Initialize bot authentication using environment variables.
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

// Create bot adapter for handling communication between bot and the service.
// See https://aka.ms/about-bot-adapter to learn more about how bots work.
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Error handler for unhandled errors during bot operations.
adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights. See https://aka.ms/bottelemetry for telemetry
    //       configuration instructions.
    // Logs the error to console. In production, you should log this to a telemetry service like Azure Application Insights.
    console.error(`\n [onTurnError] unhandled error: ${error}`);

    // Send a trace activity for debugging purposes in the Bot Framework Emulator.
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // Uncomment below commented line for local debugging.
    // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);
};

// Create the bot instance responsible for handling incoming messages.
const bot = new TeamsStartNewThreadInChannel();

// Create and configure the HTTP server using Restify.
const server = restify.createServer();
server.use(restify.plugins.bodyParser());

// Start the server to listen for incoming requests.
server.listen(process.env.port || process.env.PORT || 3978, function() {
    console.log(`\n${ server.name } listening to ${ server.url }`);
});

// Route the incoming requests to the bot for processing.
server.post('/api/messages', async (req, res) => {
    // Pass the request to the bot adapter for further processing.
    await adapter.process(req, res, (context) => bot.run(context));
});
