// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const express = require('express');

const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');

const { TeamsBot } = require('./bots/teamsBot');

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

const bot = new TeamsBot();

const server = express();
server.use(express.json());
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);
server.use("/Images", express.static(path.resolve(__dirname, 'Images')));

server.get('/scheduleTask', (req, res) => {
    res.render('./views/ScheduleTask');
});

const port = process.env.port || process.env.PORT || 3978;
server.listen(port, () =>
    console.log(`\nBot/ME service listening at http://localhost:${port}`)
);

server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, (context) => bot.run(context));
});