const path = require('path');
const express = require('express');

// Import required bot services from the Bot Framework.
// These services enable the bot to handle messages and communicate with Microsoft Teams.
const { BotFrameworkAdapter } = require('botbuilder');

// Import the bot logic from the specified file.
const { TeamsMessagingExtensionsActionBot } = require('./bots/teamsMessagingExtensionsActionBot');

// Create an instance of the Bot Framework Adapter.
// The adapter connects your bot to the Bot Framework Service and handles authentication.
const adapter = new BotFrameworkAdapter({
    appId: process.env.MicrosoftAppId,          // Microsoft App ID from Azure configuration
    appPassword: process.env.MicrosoftAppPassword // Microsoft App Password from Azure configuration
});

// Load environment variables from a .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Configure error handling for the adapter.
// This will log errors to the console and send a trace activity for debugging.
adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);

    // Send a trace activity for debugging in the Bot Framework Emulator
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // Optional: Notify the user about the error during local debugging
    // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);
};

// Instantiate the bot logic that will handle Teams messaging extensions.
const bot = new TeamsMessagingExtensionsActionBot();

// Set up the Express server for handling HTTP requests.
const server = express();

// Configure the view engine to render HTML using EJS templates.
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname); // Set the directory for EJS templates

// Define the server port, defaulting to 3978 if not specified in the environment variables.
const port = process.env.PORT || 3978;
server.listen(port, () => {
    console.log(`Server listening on http://localhost:${port}`);
});

// Log all incoming requests to the console for debugging.
server.use((req, res, next) => {
    console.log(`Request URL: ${req.url}`);
    next();
});

// Serve static files from the "public" directory.
server.use(express.static(path.join(__dirname, 'public')));

// Define a route for rendering a custom form page.
server.get('/customForm', (req, res, next) => {
    try {
        res.render('./views/CustomForm'); // Render the CustomForm EJS template
    } catch (error) {
        console.error(`Error rendering CustomForm: ${error.message}`);
        res.status(500).json({ error: 'An error occurred while rendering the CustomForm page.' });
    }
});

// Handle unmatched routes with a JSON error message.
server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});

// Configure the bot's endpoint to process incoming messages.
// This endpoint is typically used by the Microsoft Bot Framework Service.
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        // Delegate message handling to the bot logic.
        await bot.run(context);
    });
});
