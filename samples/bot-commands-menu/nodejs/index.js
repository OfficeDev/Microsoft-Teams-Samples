// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const path = require('path');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

const restify = require('restify');

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');

const { TeamsCommandsMenuBot } = require('./bots/teamsConversationBot');

// Configure bot authentication using .env variables for security
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

// Create adapter to handle interactions with the Bot Framework.
const adapter = new CloudAdapter(botFrameworkAuthentication);

/**
 * onTurnError function: Handles unhandled errors during bot execution.
 * Logs the error to the console and sends a trace activity for debugging.
 * In production, consider using Azure Application Insights for error logging.
 *
 * @param {TurnContext} context - The context for the current turn.
 * @param {Error} error - The error that occurred during the turn.
 */
adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);
    
    // Send trace activity to Bot Framework Emulator.
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );
    
    // Uncomment below line for local debugging.
    // await context.sendActivity(`Sorry, something went wrong. Exception Caught: ${error}`);
};

// Create the bot that will handle incoming messages.
const bot = new TeamsCommandsMenuBot();

/**
 * Creates and configures an HTTP server to listen for incoming requests.
 * The server listens on the specified port (either from the environment variable or defaults to 3978).
 * The server will route incoming requests to the bot adapter for processing.
 */
const server = restify.createServer();
server.use(restify.plugins.bodyParser());

server.listen(process.env.port || process.env.PORT || 3978, function() {
    console.log(`\n${server.name} listening to ${server.url}`);
});

/**
 * Listens for incoming messages and processes them through the bot.
 * Routes the request to the bot's run method for processing.
 * 
 * @param {Object} req - The incoming request object.
 * @param {Object} res - The response object.
 */
server.post('/api/messages', async (req, res) => {
    // Route received a request to the adapter for processing
    await adapter.process(req, res, (context) => bot.run(context));
});
