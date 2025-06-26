// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const path = require('path');
const express = require('express');
const cors = require('cors');

// Import required bot services and classes
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder');

// Import bot definitions
const { SentimentAnalysis } = require('./bots/sentimentAnalysis');

// Load environment variables
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Create bot authentication
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

// Create the Cloud Adapter
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Error handling middleware
adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);

    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${error}`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // You can send user-facing messages if needed
    // await context.sendActivity(`Oops! Something went wrong: ${error}`);
};

// Create the bot instance
const botActivityHandler = new SentimentAnalysis();

// Setup Express server
const server = express();
server.use(cors());
server.use(express.json());
server.use(express.urlencoded({ extended: true }));

server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

const port = process.env.port || process.env.PORT || 3978;
server.listen(port, () => {
    console.log(`\nBot/ME service listening at http://localhost:${port}`);
});

// Route for sentiment task module view
server.get('/sentimentModule', (req, res, next) => {
    res.render('./views/sentimentModule');
});

// Listen for incoming bot messages
server.post('/api/messages', async (req, res) => {
    await adapter.process(req, res, async (context) => {
        await botActivityHandler.run(context);
    });
});
