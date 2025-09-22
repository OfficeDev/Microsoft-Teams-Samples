const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');
const { BotActivityHandler } = require('../bot/botActivityHandler');

// Create an instance of the Bot Framework Authentication
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

// Create an instance of the Cloud Adapter
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Error handling: Log and trace unhandled errors
adapter.onTurnError = async (context, error) => {
    // Log error to the console for debugging purposes (in production, consider using App Insights)
    console.error(`\n [onTurnError] Unhandled error: ${error.message}`);

    // Send an error trace activity to the Bot Framework Emulator for debugging
    await context.sendTraceActivity(
        'OnTurnError Trace', // Trace activity name
        `${error.message}`,  // Error message
        'https://www.botframework.com/schemas/error', // Schema for error trace
        'TurnError'           // Activity type
    );

     // Uncomment below commented line for local debugging.
    // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);
};

// Initialize bot activity handler
const botActivityHandler = new BotActivityHandler();

/**
 * This function processes incoming requests and handles bot activities.
 * @param {object} req - The incoming HTTP request.
 * @param {object} res - The HTTP response.
 */
const botHandler = async (req, res) => {
    // Route received a request to adapter for processing
    await adapter.process(req, res, (context) => botActivityHandler.run(context));
};

module.exports = botHandler;
