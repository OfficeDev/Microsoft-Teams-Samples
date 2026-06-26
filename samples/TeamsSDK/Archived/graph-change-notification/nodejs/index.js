// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const express = require('express');

const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication,
    ConversationState,
    MemoryStorage,
    UserState
} = require('botbuilder');

const { TeamsBot } = require('./bots/teamsBot');
const { MainDialog } = require('./dialogs/mainDialog');

// Create adapter.
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);
const adapter = new CloudAdapter(botFrameworkAuthentication);

adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);

    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // Uncomment below line for local debugging.
    // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);

    await conversationState.delete(context);
};

// Define the state store for your bot.
const memoryStorage = new MemoryStorage();

// Create conversation and user state with in-memory storage provider.
const conversationState = new ConversationState(memoryStorage);
const userState = new UserState(memoryStorage);

// Create the main dialog.
const dialog = new MainDialog();
const conversationReferences = {};

// Create the bot that will handle incoming messages.
const bot = new TeamsBot(conversationState, userState, dialog, conversationReferences);

// Create HTTP server.
const server = express();
server.use(express.json());
server.use(express.urlencoded({ extended: true }));

server.listen(process.env.port || process.env.PORT || 3978, function () {
    console.log(`Service listening at port: ${this.address().port}`);
});

// Listen for incoming requests.
server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, async (context) => {
        await bot.run(context);
    });
});

// Handle Graph change notification callbacks.
const notification = async (req, res) => {
    if (req.query && req.query.validationToken) {
        res.status(200).send(req.query.validationToken);
    } else {
        const resourceData = req.body.value[0].resourceData;
        const userstatus = await dialog.getUserState('communications/presences/' + resourceData.id);

        for (const conversationReference of Object.values(conversationReferences)) {
            await adapter.continueConversationAsync(
                process.env.MicrosoftAppId,
                conversationReference,
                async (turnContext) => {
                    await bot.DisplayData(turnContext, 'User Status', userstatus.availability, userstatus.activity);
                    await turnContext.sendActivity('Change your status to get notification');
                }
            );
        }
        res.sendStatus(202);
    }
};

// Listen for incoming notification requests.
server.post('/api/notifications', notification);