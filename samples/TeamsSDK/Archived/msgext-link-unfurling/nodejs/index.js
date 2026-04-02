// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import 'dotenv/config';
import restify from 'restify';
import { CloudAdapter, ConfigurationBotFrameworkAuthentication } from 'botbuilder';
import { TeamsLinkUnfurlingBot } from './bots/teamsLinkUnfurlingBot.js';

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

const bot = new TeamsLinkUnfurlingBot();

const server = restify.createServer();
server.use(restify.plugins.bodyParser());

server.listen(process.env.port || process.env.PORT || 3978, () => {
    console.log(`\nServer listening on ${server.url}`);
});

server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, (context) => bot.run(context));
});
