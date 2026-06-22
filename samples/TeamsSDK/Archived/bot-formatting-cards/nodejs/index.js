// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const express = require('express');
const { CloudAdapter, ConfigurationBotFrameworkAuthentication } = require('botbuilder');
const { BotFormattingCards } = require('./bots/botFormattingCards');

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

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
};

const botActivityHandler = new BotFormattingCards();

const server = express();
server.use(express.json());
const port = process.env.port || process.env.PORT || 3978;
server.listen(port, () =>
    console.log(`Bot/ME service listening at http://localhost:${port}`)
);

server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, (context) => botActivityHandler.run(context));
});