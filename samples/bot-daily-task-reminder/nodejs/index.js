const path = require('path');
const express = require('express');
const cors = require('cors');
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE }); // Load environment variables from .env file
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

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { BotFrameworkAdapter } = require('botbuilder');
const { TeamsBot } = require('./bots/teamsBot');

// Create adapter with bot credentials from environment variables
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new BotFrameworkAdapter({
    appId: process.env.MicrosoftAppId,
    appPassword: process.env.MicrosoftAppPassword
});

// Error handling for the bot adapter
adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights. See https://aka.ms/bottelemetry for telemetry
    //       configuration instructions.
    console.error(`\n [onTurnError] unhandled error: ${error}`);
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );
    // Uncomment below commented line for local debugging..
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

// Route for rendering the schedule task page
server.get('/scheduleTask', (req, res) => {
    res.render('./views/ScheduleTask');
});

// Route for handling 404 errors
server.get('*', (req, res) => {
    res.json({ error: 'Route not found. Please check the endpoint.' });
});

// Route for handling bot messages
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        await bot.run(context);
    });
});
