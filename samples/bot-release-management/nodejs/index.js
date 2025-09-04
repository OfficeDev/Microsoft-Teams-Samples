// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const path = require('path');

// Note: Ensure you have a .env file and include the MicrosoftAppId and MicrosoftAppPassword.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

const express = require('express');
const cors = require('cors');
global.taskDetails = {};

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');

// This bot's main dialog.
const { ActivityBot } = require('./bots/activityBot');
const { DevOpsHelper } = require('./helpers/devOpsHelper');
const GraphHelper = require('./helpers/graphHelper');
const { Constant } = require('./models/constant');

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

const adapter = new CloudAdapter(botFrameworkAuthentication);

// Catch-all for errors.
adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights. See https://aka.ms/bottelemetry for telemetry 
    //       configuration instructions.
    //console.log(context);
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

// Create the main dialog.

const bot = new ActivityBot();

// Create HTTP server.
const server = express();
server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));

server.listen(process.env.port || process.env.PORT || 3978, function () {
    console.log(`\n${server.name} listening to ${server.url}`);
    console.log('\nGet Bot Framework Emulator: https://aka.ms/botframework-emulator');
    console.log('\nTo talk to your bot, open the emulator select "Open Bot"');
});

// Listen for incoming activities and route them to your bot main dialog.
server.post('/api/messages', async (req, res) => {
    // Route received a request to adapter for processing
    await adapter.process(req, res, (context) => bot.run(context));
});

// Listen for incoming notifications and send proactive messages to group.
server.post('/api/workItem', async (req, res) => {
    // Maps incoming workitem payload to release management model.
    var releaseManagementTask = await DevOpsHelper.MapToReleaseManagementTask(req.body);

    taskDetails[Constant.TaskDetails] = releaseManagementTask;

    if (releaseManagementTask.GroupChatMembers.length > 1)
    {
        var graphHelper = new GraphHelper();
        var groupChat = await graphHelper.CreateGroupChatAsync(releaseManagementTask.GroupChatMembers, releaseManagementTask.TaskTitle);
        await graphHelper.AppinstallationforGroupAsync(groupChat.id);
    }
    return res.status(200).send();
});