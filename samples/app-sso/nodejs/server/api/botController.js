// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { ConversationState, MemoryStorage, UserState } = require('botbuilder');
const { BotActivityHandler } = require('../bots/botActivityHandler');
const { MainDialog } = require('../dialogs/mainDialog');

const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');

// Create authentication object
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Error handling middleware
adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights. See https://aka.ms/bottelemetry for telemetry
    //       configuration instructions.
    console.error(`\n [onTurnError] unhandled error: ${ error }`);

    // Send error trace activity
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // Uncomment for local debugging
    // await context.sendActivity(`Oops! Something went wrong: ${error.message}`);

    // Clear conversation state to prevent the bot from getting stuck
    await conversationState.delete(context);
};

// Define the state store for your bot.
// See https://aka.ms/about-bot-state to learn more about using MemoryStorage.
// A bot requires a state storage system to persist the dialog and user state between messages.
const memoryStorage = new MemoryStorage();

// Create conversation and user state with in-memory storage provider.
const conversationState = new ConversationState(memoryStorage);
const userState = new UserState(memoryStorage);

// Create the main dialog and bot handler
const dialog = new MainDialog();
const bot = new BotActivityHandler(conversationState, userState, dialog);

// Express route handler for bot messages
const botHandler = (req, res) => {
    adapter.process(req, res, async (context) => {
        await bot.run(context);
    });
};

module.exports = botHandler;
