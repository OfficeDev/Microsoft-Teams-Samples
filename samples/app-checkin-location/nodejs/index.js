// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const path = require('path');
const express = require('express');

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');

// Import bot definitions
const { TeamsBot } = require('./bots/teamsBot');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

const adapter = new CloudAdapter(botFrameworkAuthentication);

adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights.
    console.error(`\n [onTurnError] unhandled error: ${ error }`);

    // Send a trace activity, which will be displayed in Bot Framework Emulator
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${ error }`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

     // Uncomment below commented line for local debugging.
     // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);    
};

// Create bot handlers
const botActivityHandler = new TeamsBot();

// Create HTTP server.
const server = express();
server.use(express.json());
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

const port = process.env.port || process.env.PORT || 3978;

server.listen(port, () => 
    console.log(`\Bot/ME service listening at http://localhost:${port}`)
);
server.use("/Images", express.static(path.resolve(__dirname, 'Images')));

server.get('/CheckIn', (req, res, next) => {
    res.render('./views/CheckIn')
});

server.get('/ViewLocation', (req, res, next) => {
    res.render('./views/ViewLocation')
});

server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});

// Listen for incoming requests.
server.post('/api/messages', async (req, res) => {
    // Route received a request to adapter for processing
    await adapter.process(req, res, (context) => botActivityHandler.run(context));
});