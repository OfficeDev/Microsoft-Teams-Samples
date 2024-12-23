const path = require('path');
const express = require('express');

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { BotFrameworkAdapter } = require('botbuilder');

const { TeamsMessagingExtensionsActionBot } = require('./bots/teamsMessagingExtensionsActionBot');

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new BotFrameworkAdapter({
    appId: process.env.MicrosoftAppId,
    appPassword: process.env.MicrosoftAppPassword
});

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

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
};

// Create the bot that will handle incoming messages.
const bot = new TeamsMessagingExtensionsActionBot();

const server = express();
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

const port = process.env.PORT || 3978;
server.listen(port, () => {
    console.log(`Server listening on http://localhost:${port}`);
});

server.use("/Images", express.static(path.resolve(__dirname, 'Images')));

server.get('/customForm', (req, res, next) => {
    res.render('./views/CustomForm')
});

server.get('/staticPage', (req, res, next) => {
    res.render('./views/StaticPage')
});

server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});

server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        await bot.run(context);
    });
});