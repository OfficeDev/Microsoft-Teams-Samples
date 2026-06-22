const path = require('path');
const express = require('express');
const cors = require('cors');
require('dotenv').config({ path: path.join(__dirname, '.env') });

const { ConfigurationBotFrameworkAuthentication, CloudAdapter, MemoryStorage, ConversationState, UserState } = require('botbuilder');
const { TeamsBot } = require('./bots/teamsBot');
const { MainDialog } = require('./dialogs/mainDialog');

const PORT = process.env.PORT || 3978;
const server = express();

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({ extended: true }));

// Bot authentication and adapter setup using CloudAdapter
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Define error handler
adapter.onTurnError = async (context, error) => {
    console.error(`[onTurnError] unhandled error: ${error}`);
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );
    await conversationState.delete(context);
};

// Memory storage and state management
const memoryStorage = new MemoryStorage();
const conversationState = new ConversationState(memoryStorage);
const userState = new UserState(memoryStorage);

// Bot logic setup
const dialog = new MainDialog(conversationState);
const bot = new TeamsBot(conversationState, userState, dialog);

// Static files
server.use('/Images', express.static(path.resolve(__dirname, 'Images')));

// Endpoint for incoming bot messages
server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, async (context) => {
        await bot.run(context);
    });
});

// Start server
server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});
