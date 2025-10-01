// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const express = require('express');

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { CloudAdapter, ConfigurationBotFrameworkAuthentication } = require('botbuilder');

const { TeamsMessagingExtensionsActionBot } = require('./bots/teamsMessagingExtensionsActionBot');

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);
const adapter = new CloudAdapter(botFrameworkAuthentication);

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

// Add this line so req.body is populated
server.use(express.json());

server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

const port = process.env.PORT || 3978;
server.listen(port, () => {
    console.log(`Server listening on http://localhost:${port}`);
});

server.use("/Images", express.static(path.resolve(__dirname, 'Images')));

server.get('/customForm', (req, res, next) => {
    res.render('./views/CustomForm');
});

server.get('/staticPage', (req, res, next) => {
    res.render('./views/StaticPage');
});

server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});

// Listen for incoming requests.
server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, (context) => bot.run(context));
});