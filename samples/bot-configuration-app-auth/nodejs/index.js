const path = require('path');
const express = require('express');
const cors = require('cors');
const dotenv = require('dotenv');
const { BotFrameworkAdapter } = require('botbuilder');
const { TeamsBot } = require('./teamsBot');
const config = require("./config");

// Load environment variables from .env file
dotenv.config({ path: path.join(__dirname, '.env') });

const PORT = process.env.PORT || 3978;
const server = express();

// Middleware setup
server.use(cors());
server.use(express.json());
server.use(express.urlencoded({ extended: true }));

// Create adapter
const adapter = new BotFrameworkAdapter({
    appId: config.botId,
    appPassword: config.botPassword
});

// Error handling for the adapter
adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);

    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    await context.sendActivity('The bot encountered an error or bug.');
    await context.sendActivity('To continue to run this bot, please fix the bot source code.');
};

// Create the bot that will handle incoming messages
const bot = new TeamsBot();

// Start the server
server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});

// Serve static images
server.use("/Images", express.static(path.resolve(__dirname, 'Images')));

// Handle undefined routes
server.get('*', (req, res) => {
    res.status(404).json({ error: 'Route not found' });
});

// Endpoint for bot messages
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        await bot.run(context);
    });
});