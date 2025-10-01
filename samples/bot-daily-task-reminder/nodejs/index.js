const path = require('path');
const express = require('express');
const cors = require('cors');
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');
const { TeamsBot } = require('./bots/teamsBot');
require('dotenv').config({ path: path.join(__dirname, '.env') }); // Load environment variables from .env file

const PORT = process.env.PORT || 3978;
const server = express();

// Middleware setup
server.use(cors());
server.use(express.json());
server.use(express.urlencoded({ extended: true }));

// Set up EJS as the templating engine
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

// Create adapter with bot credentials from environment variables
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Error handling for the bot adapter
adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );
    // Uncomment below line for local debugging
    // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);
};

// Create an instance of the bot
const bot = new TeamsBot();

// Start the Express server
server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});

// Serve static images from the "Images" directory
server.use("/Images", express.static(path.resolve(__dirname, 'Images')));

/**
 * Route for rendering the schedule task page
 */
server.get('/scheduleTask', (req, res) => {
    res.render('./views/ScheduleTask');
});

/**
 * Route for handling 404 errors
 */
server.get('*', (req, res) => {
    res.status(404).json({ error: 'Route not found. Please check the endpoint.' });
});

/**
 * Route for handling bot messages
 */
server.post('/api/messages', async (req, res) => {
    // Route received a request to adapter for processing
    await adapter.process(req, res, async (context) => {
        await bot.run(context);
    });
});