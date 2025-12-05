// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// File: index.js
// Purpose: Minimal Express host for the Teams messaging extension (link unfurl + search).
// Key Steps: load env, create CloudAdapter, instantiate bot, expose /api/messages.

// Import required packages
const path = require('path');

// Load environment variables from .env (auth settings, etc.)
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

const express = require('express');

// Import CloudAdapter + auth loader (Agents SDK)
const {
    CloudAdapter,
    loadAuthConfigFromEnv
} = require('@microsoft/agents-hosting');

const { TeamsLinkUnfurlingBot } = require('./bots/teamsLinkUnfurlingBot');

// Load authentication configuration (uses standard env vars defined by Agents SDK)
const authConfig = loadAuthConfigFromEnv();

// Create CloudAdapter instance
const adapter = new CloudAdapter(authConfig);

adapter.onTurnError = async (context, error) => {
    // Minimal error handler: log + trace (for local debugging / emulator).
    console.error('[onTurnError]', error);
    await context.sendTraceActivity('OnTurnError Trace', String(error), 'https://www.botframework.com/schemas/error', 'TurnError');
    // Optionally send a user-friendly activity (commented out to avoid noisy UX):
    // await context.sendActivity('Sorry, something went wrong.');
};

// Instantiate bot
const bot = new TeamsLinkUnfurlingBot();

// Express server setup
const app = express();
app.use(express.json());

// Bot endpoint (Teams invokes hit this route)
app.post('/api/messages', async (req, res) => {
    try {
        await adapter.process(req, res, (context) => bot.run(context));
    } catch (e) {
        console.error('[Express] Error processing /api/messages:', e);
        if (!res.headersSent) {
            res.status(500).send({ error: 'Processing error' });
        }
    }
});

const port = process.env.port || process.env.PORT || 3978;
app.listen(port, () => {
    console.log(`Express server listening on http://localhost:${port}`);
});
