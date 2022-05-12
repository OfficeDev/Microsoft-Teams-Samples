// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import path from 'path';
import restify from 'restify';
import { adapter, EchoBot } from './bot.js';
import tabs from './tabs.js';
import MessageExtension from './message-extension.js';
import { fileURLToPath } from 'url';
import dotenv from 'dotenv';

// See https://aka.ms/bot-services to learn more about the different parts of a bot.
import { ActivityTypes } from 'botbuilder';

// Read botFilePath and botFileSecret from .env file.
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });

//Create HTTP server.
const server = restify.createServer({
    formatters: {
        'text/html': function (req, res, body) {
            return body;
        },
    },
});

server.get(
    '/*',
    restify.plugins.serveStatic({
        directory: __dirname + '/static',
    })
);

server.listen(process.env.port || process.env.PORT || 3333, function () {
    console.log(`\n${server.name} listening to ${server.url}`);
});

// Adding tabs to our app. This will setup routes to various views
tabs(server);

// Adding a bot to our app
const bot = new EchoBot();

// Adding a messaging extension to our app
const messageExtension = new MessageExtension();

// Listen for incoming requests.
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        if (context.activity.type === ActivityTypes.Invoke)
            await messageExtension.run(context);
        else await bot.run(context);
    });
});

const server = restify.createServer({
    formatters: {
        'text/html': function (req, res, body) {
            return body;
        },
    },
});

server.get(
    '/*',
    restify.plugins.serveStatic({
        directory: __dirname + '/static',
    })
);

server.listen(process.env.port || process.env.PORT || 3333, function () {
    console.log(`\n${server.name} listening to ${server.url}`);
});

// Adding tabs to our app. This will setup routes to various views
tabs(server);

// Adding a bot to our app
const bot = new EchoBot();

// Adding a messaging extension to our app
const messageExtension = new MessageExtension();

// Listen for incoming requests.
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        if (context.activity.type === ActivityTypes.Invoke)
            await messageExtension.run(context);
        else await bot.run(context);
    });
});
