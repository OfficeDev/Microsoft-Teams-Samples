// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import path from 'path';
import restify from 'restify';
import { adapter, EchoBot } from './bot';
import tabs from './tabs';
import MessageExtension from './message-extension';
import { ActivityTypes } from 'botbuilder';

// Read environment variables from .env file for bot credentials and settings.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Create an HTTP server using Restify.
const server = restify.createServer({
    formatters: {
        'text/html': (req, res, body) => body, // Return body as-is for HTML responses.
    },
});

// Serve static files (e.g., for web pages or resources like images).
server.get(
    '/*',
    restify.plugins.serveStatic({
        directory: path.join(__dirname, 'static'),
    })
);

// Start the server on the configured port, falling back to 3978 if not set.
server.listen(process.env.port || process.env.PORT || 3978, () => {
    console.log(`${server.name} listening to ${server.url}`);
});

// Initialize tabs and message extension functionalities.
tabs(server); // Setup routes for tab functionality in the bot.
const bot = new EchoBot(); // Initialize the EchoBot to handle user interactions.
const messageExtension = new MessageExtension(); // Initialize message extension for bot.

server.post('/api/messages', (req, res, next) => {
    // Process incoming activity and route to either bot or message extension based on activity type.
    adapter.processActivity(req, res, async (context) => {
        if (context.activity.type === ActivityTypes.Invoke) {
            await messageExtension.run(context); // Handle Invoke activities (e.g., message extensions).
        } else {
            await bot.run(context); // Handle other types of activities (e.g., user messages).
        }
        return next(); // Continue processing any other middleware.
    });
});
