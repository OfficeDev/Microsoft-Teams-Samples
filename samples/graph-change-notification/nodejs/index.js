// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Import required pckages
const path = require('path');
// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const express = require('express');

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { ConversationState, MemoryStorage, UserState, CardFactory } = require('botbuilder');

const { TeamsBot } = require('./bots/teamsBot');
const { MainDialog } = require('./dialogs/mainDialog');
const { DialogBot } = require('./bots/dialogBot');


// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder')



CloudAdapter.onTurnError = async (context, error) => {
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
const dialog = new MainDialog();
// Create the main dialog.
const conversationReferences = {};
// Create the bot that will handle incoming messages.
const bot = new TeamsBot(conversationState, userState, dialog, conversationReferences);

// Create HTTP server.
const server = express();
server.use(express.json());

server.use(express.urlencoded({
    extended: true
}));

server.listen(process.env.port || process.env.PORT || 3978, function () {
    console.log(`Service listening at:${process.env.port}`);
});

// Listen for incoming requests.
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        await bot.run(context);
    });
});

const notification = async (req, res, next) => {
    let status;
    if (req.query && req.query.validationToken) {
        console.log("In Controller");
        status = 200;
        res.send(req.query.validationToken);
    } else {
        console.log("In Response");
        clientStatesValid = false;
        console.log(req.body.value[0].resourceData);
        
        // Call the API
        const userstatus = await dialog.getUserState("communications/presences/" + req.body.value[0].resourceData.id);
        status = 202;
        //for storing step context
        const dbot = new DialogBot(conversationState, userState, dialog, conversationReferences);
        const _notication = "Change your status to get notification";
        for (const conversationReference of Object.values(conversationReferences)) {
            await adapter.continueConversation(conversationReference, async turnContext => {
                let carddata = await dbot.DisplayData(turnContext, "User Status", userstatus.availability, userstatus.activity);
                await turnContext.sendActivity(_notication);
            });
        }
        res.sendStatus(status);
    }
}

// Listen for incoming requests.
server.post('/api/notifications', notification);