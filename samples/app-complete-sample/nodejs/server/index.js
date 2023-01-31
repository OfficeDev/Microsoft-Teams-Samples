const path = require('path');
const express = require('express');
const cors = require('cors');
const fs = require('fs')
const ENV_FILE = path.join(__dirname, '../.env');
require('dotenv').config({ path: ENV_FILE });
const PORT = process.env.PORT || 3978;
const server = express();

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));
server.use(express.static(path.join(__dirname, "../../public")));
server.use(express.static(path.join(__dirname, "./public")));
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);



// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { BotFrameworkAdapter, ConversationState, MemoryStorage, UserState } = require('botbuilder');

const { Bot } = require('./bot/Bot');
const { RootDialog } = require('./dialogs/rootDialog');

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new BotFrameworkAdapter({
    appId: process.env.MicrosoftAppId,
    appPassword: process.env.MicrosoftAppPassword
});

adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights. See https://aka.ms/bottelemetry for telemetry 
    //       configuration instructions.
    console.error(`\n [onTurnError] unhandled error: ${error}`);

    // Send a trace activity, which will be displayed in Bot Framework Emulator
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // Uncomment below commented line for local debugging.
    // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);

    // Clear out state
    await conversationState.delete(context);
};

// Define the state store for your bot.
// See https://aka.ms/about-bot-state to learn more about using MemoryStorage.
// A bot requires a state storage system to persist the dialog and user state between messages.
const memoryStorage = new MemoryStorage();

// Create conversation and user state with in-memory storage provider.
const conversationState = new ConversationState(memoryStorage);
const userState = new UserState(memoryStorage);

// Create the main dialog.
const dialog = new RootDialog(conversationState);
// Create the bot that will handle incoming messages.
const bot = new Bot(conversationState, userState, dialog);


// Listen for incoming requests.
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        await bot.run(context);
    });
});

server.get('/configure', function (req, res) {
    res.render('./views/configure');
});
server.get('/botInfo', function (req, res) {
    var fileContent;
    fs.readFile('./server/dialogs/rootDialog.js', (err, data) => {
        if (err) throw err;
        fileContent = data.toString();
        res.render('./views/botInfo', { fileContent: JSON.stringify(fileContent) });
    });

});
server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});
