const path = require('path');
const express = require('express');
const cors = require('cors');

// Load environment variables from Teams Toolkit environment files
const TEAMSFX_ENV = process.env.TEAMSFX_ENV || 'local';
require('dotenv').config({ path: path.join(__dirname, 'env', `.env.${TEAMSFX_ENV}`) });
require('dotenv').config({ path: path.join(__dirname, 'env', `.env.${TEAMSFX_ENV}.user`) });

const PORT = process.env.PORT || 3978;
const server = express();

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);
server.use(express.static(path.join(__dirname, 'static')));

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');

const { TeamsBot } = require('./bots/teamsBot');

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

const adapter = new CloudAdapter(botFrameworkAuthentication);

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

    // Send a message to the user
    await context.sendActivity('The bot encountered an error or bug.');
    await context.sendActivity('To continue to run this bot, please fix the bot source code.');
};

// Create the bot that will handle incoming messages.
const bot = new TeamsBot();

server.listen(PORT, () => {
    console.log('Server listening on port: ' + PORT);
});

server.use("/Images", express.static(path.resolve(__dirname, 'Images')));

// Endpoint to fetch Link unfurling tab page.
server.get('/tab', (req, res, next) => {
  res.render('./views/tab')
});

server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});

server.post('/api/messages', async (req, res) => {
    // Route received a request to adapter for processing
    await adapter.process(req, res, (context) => bot.run(context));
});

