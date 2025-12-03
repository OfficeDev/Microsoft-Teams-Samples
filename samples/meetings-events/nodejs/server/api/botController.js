const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication,
    MemoryStorage,
    ConversationState
} = require('botbuilder');
const { BotActivityHandler } = require('../bot/botActivityHandler');

const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

const adapter = new CloudAdapter(botFrameworkAuthentication);

adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. server insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       serverlication insights.
    console.error(`\n [onTurnError] unhandled error: ${ error }`);

    // Send a trace activity, which will be displayed in Bot Framework Emulator
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${ error }`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

     // Uncomment below commented line for local debugging.
     // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);

};

// Define state store for your bot.
// See https://aka.ms/about-bot-state to learn more about bot state.
const memoryStorage = new MemoryStorage();

// Create conversation and user state with in-memory storage provider.
const conversationState = new ConversationState(memoryStorage);

// Create bot handlers
const botActivityHandler = new BotActivityHandler(conversationState);
const botHandler = async (req, res) => {
    // Route received a request to adapter for processing
    await adapter.process(req, res, (context) => botActivityHandler.run(context));
};

module.exports = botHandler;