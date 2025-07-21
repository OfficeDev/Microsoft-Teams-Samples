const { BotFrameworkAdapter } = require('botbuilder');
const { BotActivityHandler } = require('../bot/botActivityHandler');

// Create an instance of the Bot Framework Adapter
const adapter = new BotFrameworkAdapter({
    appId: process.env.BotId,           // Bot's App ID
    appPassword: process.env.BotPassword // Bot's App Password
});

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
const botHandler = (req, res) => {
    // Process incoming activity with the Bot Framework Adapter
    adapter.processActivity(req, res, async (context) => {
        try {
            // Handle the activity using the bot's activity handler
            await botActivityHandler.run(context);
        } catch (error) {
            console.error('Error processing bot activity:', error);
            // Optionally, send an error response back to the user or log it for debugging.
        }
    });
};

module.exports = botHandler;
