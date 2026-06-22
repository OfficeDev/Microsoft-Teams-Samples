// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
require('dotenv').config({ path: path.join(__dirname, '.env') });

const restify = require('restify');

const { CloudAdapter, ConfigurationBotFrameworkAuthentication } = require('botbuilder');
const { TeamsStartNewThreadInChannel } = require('./bots/TeamsStartNewThreadInChannel');

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

const bot = new TeamsStartNewThreadInChannel();

const server = restify.createServer();
server.use(restify.plugins.bodyParser());

server.listen(process.env.port || process.env.PORT || 3978, function() {
    console.log(`\n${ server.name } listening to ${ server.url }`);
});

server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, (context) => bot.run(context));
});
