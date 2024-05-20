// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// index.js is used to setup and configure your bot
// Import required pckages
const path = require('path');
const cors = require('cors');
const express = require('express');
const app = express();

// Create HTTP server.
const server = require('http').createServer(app);
const io = require('socket.io')(server, { cors: { origin: "*" } });

app.use(cors());
app.use(express.json());
app.use(express.urlencoded({
    extended: true
}));

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { BotFrameworkAdapter } = require('botbuilder');
const { PersonalBot } = require('./bots/PersonalBot');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '..', '.env');
require('dotenv').config({ path: ENV_FILE });

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new BotFrameworkAdapter({
    appId: process.env.MicrosoftAppId,
    appPassword: process.env.MicrosoftAppPassword
});

adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights.
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
const bot = new PersonalBot();

// Socket Interaction To Send Message 
io.on('connection', (socket) => {
    // send a message to the client
    io.emit('message', PersonalBot.conversationArray);
});

// Socket Interaction To Receive message
io.on('connection', (socket) => {
    socket.on('message', msg => {
        console.log("Message received from tab ", msg);
        adapter.sendActivities = async (context, activity) => {
            // send a message to the user.
            await context.sendActivity(`Message From Tab : ${msg}`);
        };
    });
});

const port = 3000;
server.listen(port || process.env.PORT || 3000, function () {
    console.log(`\n${server.name} listening to ${port}`);
});

// Listen for incoming requests.
app.post('/api/messages', (req, res, next) => {
    adapter.processActivity(req, res, async (context) => {
        globalContext = context;
        await bot.run(context);
        return next();
    });
});








