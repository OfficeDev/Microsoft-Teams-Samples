// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const express = require('express');
const cors = require('cors');
const { CloudAdapter, loadAuthConfigFromEnv, authorizeJWT } = require('@microsoft/agents-hosting');
const ENV_FILE = path.join(__dirname, '../.env');
require('dotenv').config({ path: ENV_FILE });

const PORT = process.env.PORT || 3978;
const server = express();

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));
server.engine('.html', require('ejs').renderFile);
server.set('views', __dirname);
server.set('view engine', 'ejs');

// Import the agent
const { agent } = require('./api/botController');

// Load authentication configuration from environment variables
const authConfig = loadAuthConfigFromEnv();

// Create adapter using the Agents SDK
const adapter = new CloudAdapter(authConfig);

// Error handler for the adapter
adapter.onTurnError = async (context, error) => {
    console.error(`[onTurnError] unhandled error: ${error}`);
    console.error(error.stack);
    try {
        await context.sendActivity('The bot encountered an error. Please try again.');
    } catch (e) {
        console.error('Error sending error message:', e);
    }
};

// Apply JWT authorization middleware for secure communication
server.use('/api/messages', authorizeJWT(authConfig));

// Handle bot messages
server.post('/api/messages', async (req, res) => {
    try {
        console.log('Received activity:', req.body?.type, 'Text:', req.body?.text);
        await adapter.process(req, res, (context) => agent.run(context));
    } catch (error) {
        console.error('Error processing message:', error);
        res.status(500).send('Internal Server Error');
    }
});

server.get('/content', (req, res, next) => {
    res.sendFile('views/content-tab.html', {root: __dirname })
});

server.get('/tab', (req, res, next) => {
    res.render('./views/sampleTab', { teamsAppId: process.env.TeamsAppId || process.env.TEAMS_APP_ID, baseUrl: process.env.BaseUrl || process.env.BOT_ENDPOINT });
});

server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});

server.listen(PORT, () => {
    console.log('Server listening on port: ' + PORT);
});
