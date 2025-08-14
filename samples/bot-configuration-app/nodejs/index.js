const path = require('path');
const express = require('express');
const cors = require('cors');
const { BotFrameworkAdapter } = require('botbuilder');
const { TeamsBot } = require('./teamsBot');
const config = require("./config");

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

const PORT = process.env.PORT || 3978;
const server = express();

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({ extended: true }));

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new BotFrameworkAdapter({
    appId: config.botId,
    appPassword: config.botPassword
});

/**
 * Handles errors that occur during the bot's turn.
 * @param {Object} context - The context object.
 * @param {Error} error - The error object.
 */
const onTurnErrorHandler = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);

    // Send a trace activity, which will be displayed in Bot Framework Emulator
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // Send a message to the user
    await context.sendActivity('The bot encountered an error or bug.');
    await context.sendActivity('To continue to run this bot, please fix the bot source code.');
};

adapter.onTurnError = onTurnErrorHandler;

// Create the bot that will handle incoming messages.
const bot = new TeamsBot();

server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});

server.use("/Images", express.static(path.resolve(__dirname, 'Images')));

server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});

server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        await bot.run(context);
    });
});