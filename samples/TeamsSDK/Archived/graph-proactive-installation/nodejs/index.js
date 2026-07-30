// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
require('dotenv').config({ path: path.join(__dirname, '.env') });

const express = require('express');
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');

const { ProactiveBot } = require('./bots/proactiveBot');

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

    // Uncomment below commented line for local debugging.
    // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);
};

const conversationReferences = {};
const bot = new ProactiveBot(conversationReferences);

const server = express();
const port = process.env.port || process.env.PORT || 3978;
server.listen(port, () =>
    console.log(`Bot service listening at http://localhost:${port}`)
);
server.use('/Images', express.static(path.resolve(__dirname, 'Images')));
server.use(express.json());

server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, (context) => bot.run(context));
});

server.get('/api/notify', async (req, res) => {
    for (const conversationReference of Object.values(conversationReferences)) {
        await adapter.continueConversationAsync(
            process.env.MicrosoftAppId,
            conversationReference,
            async (turnContext) => {
                await turnContext.sendActivity('proactive hello');
            }
        );
    }

    res.status(200).type('html').send('<html><body><h1>Proactive messages have been sent.</h1></body></html>');
});
