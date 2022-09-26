// <copyright file="index.js" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

// Import required pckages
const path = require('path');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const restify = require('restify');
const express = require('express');

const { MeetingNotficationBot } = require('./bots/meeting-notification-bot');
const { DecryptionHelper } = require("./helper/decryption-helper");

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { CloudAdapter, 
    ConversationState, 
    MemoryStorage, 
    ConfigurationServiceClientCredentialFactory, 
    createBotFrameworkAuthenticationFromConfiguration } = require('botbuilder');

const credentialsFactory = new ConfigurationServiceClientCredentialFactory({
    MicrosoftAppId: process.env.MicrosoftAppId,
    MicrosoftAppPassword: process.env.MicrosoftAppPassword,
    MicrosoftAppTenantId: process.env.TenantId
  });
  
  const botFrameworkAuthentication = createBotFrameworkAuthenticationFromConfiguration(null, credentialsFactory);


// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
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
    // Clear out state
    await conversationState.delete(context);
};

// Define the state store for your bot.
// See https://aka.ms/about-bot-state to learn more about using MemoryStorage.
// A bot requires a state storage system to persist the dialog and user state between messages.
const memoryStorage = new MemoryStorage();

// Create conversation and user state with in-memory storage provider.
const conversationState = new ConversationState(memoryStorage);

// Create the main dialog.
//const dialog = new MainDialog();
// Create the main dialog.
const conversationReferences = {};

// Create the bot that will handle incoming messages.
const bot = new MeetingNotficationBot(conversationReferences);

// Create HTTP server.
const server = express();
server.use(restify.plugins.queryParser());
server.use(express.json());

server.use(express.urlencoded({
    extended: true
}));

server.listen(process.env.port || process.env.PORT || 3978, function () {
    console.log(`\n${server.name} listening to ${server.url}`);
    console.log('\nGet Bot Framework Emulator: https://aka.ms/botframework-emulator');
    console.log('\nTo talk to your bot, open the emulator select "Open Bot"');
});

// Listen for incoming requests.
server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, (context) => bot.run(context));
});

// This method will be invoked for various meeting events.
const notification = async (req, res, next) => {
    let status;

    if (req.query && req.query.validationToken) {
        status = 200;
        res.send(req.query.validationToken);
    } else {
        clientStatesValid = false;
        var responsePayload = await DecryptionHelper.getDecryptedContent(req.body.value);

        if(responsePayload.eventType == "Microsoft.Communication.CallRosterUpdate") {
            if(responsePayload['activeParticipants@delta'].length > 0 || responsePayload['activeParticipants@remove'].length > 0) {
                for (const conversationReference of Object.values(conversationReferences)) {
                    await adapter.continueConversationAsync(process.env.MicrosoftAppId, conversationReference, async turnContext => {

                        if(responsePayload['activeParticipants@delta'].length > 0) {
                            var membersJoined = new Array();
                            responsePayload['activeParticipants@delta'].map((member) => {
                                var member = {
                                    title: "Member name",
                                    value: member.Identity.User.DisplayName
                                 }
                               membersJoined.push(member);
                            })

                            await MeetingNotficationBot.DisplayMeetingUpdate(turnContext, "Members joined",membersJoined);
                        }

                        if(responsePayload['activeParticipants@remove'].length > 0) {
                            var membersLeft = new Array();
                            responsePayload['activeParticipants@remove'].map((member) => {
                               var member = {
                               title: "Member name",
                               value: member.Identity.User.DisplayName
                            }
                               membersLeft.push(member);
                            })

                            await MeetingNotficationBot.DisplayMeetingUpdate(turnContext, "Members left", membersLeft);
                        }
                    });
                }
            }
        }
        else {
            var meetingIndicator = responsePayload.eventType == "Microsoft.Communication.CallStarted" ? "Meeting has been started." : "Meeting has been ended.";
            for (const conversationReference of Object.values(conversationReferences)) {
                await adapter.continueConversationAsync(process.env.MicrosoftAppId, conversationReference, async turnContext => {
                    let carddata = await MeetingNotficationBot.MeetingStartEndCard(turnContext, meetingIndicator);
                });
            }
        }

        res.status(202).send();
    }
}

// Listen for incoming meeting updates.
server.post('/api/notifications', notification);