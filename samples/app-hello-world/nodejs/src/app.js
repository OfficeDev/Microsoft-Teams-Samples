// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/**
 * This sample demonstrates a simple Teams Agent / Copilot app using
 * the Microsoft Agents SDK (@microsoft/agents-hosting).
 *
 * Key points:
 *  - No dependency on 'botbuilder'
 *  - Uses CloudAdapter from Agents Hosting SDK
 *  - Supports message extensions and tab routes
 *  - Works for Teams agents or Copilot extensibility scenarios
 */

import path from "path";
import express from "express";
import dotenv from "dotenv";
import { fileURLToPath } from "url";
import { EchoBot } from "./bot.js";
import tabs from "./tabs.js";

// Import from @microsoft/agents-hosting (CommonJS module)
import AgentsHosting from "@microsoft/agents-hosting";

const { CloudAdapter, MemoryStorage, ConversationState, loadPrevAuthConfigFromEnv } = AgentsHosting;

// Load environment variables from .env file
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
dotenv.config({ path: path.join(__dirname, "../.env") });

// Initialize adapter and conversation state
const memoryStorage = new MemoryStorage();
const conversationState = new ConversationState(memoryStorage);

// Create adapter.
// Prefer new env variable names when available, otherwise fall back to classic bot settings.
const authConfig = process.env.clientId ? undefined : loadPrevAuthConfigFromEnv();
const adapter = new CloudAdapter(authConfig);

// Global error handler for adapter
adapter.onTurnError = async (context, error) => {
    console.error(`[onTurnError] Unhandled error: ${error}`);
    console.error(error.stack);

    try {
        // Reset conversation state if something went wrong
        await conversationState.delete(context);
    } catch (err) {
        console.error(`Error clearing conversation state: ${err}`);
    }

    // Send error message to the user
    await context.sendActivity("The agent encountered an error. Please try again later.");
};

// Create and configure the HTTP server using Express
const server = express();

// Enable JSON and URL-encoded body parsing middleware
server.use(express.json());
server.use(express.urlencoded({ extended: true }));

// Serve static files such as tab pages or assets
server.use(express.static(path.join(__dirname, "static")));

// Simple health check endpoint for diagnostics
server.get('/healthz', (req, res) => {
    res.json({ status: 'ok', time: new Date().toISOString() });
});

// Start the web server
const port = process.env.PORT || 3978;
server.listen(port, () => {
    console.log(`Server listening on http://localhost:${port}`);
});

// Initialize bot logic (now includes message extension support)
const bot = new EchoBot();

// Register tab routes
tabs(server);

// Handle incoming Teams or Copilot activities
server.post("/api/messages", async (req, res) => {
    await adapter.process(req, res, async (context) => {
        const originalSendActivity = context.sendActivity.bind(context);
        let replied = false;
        context.sendActivity = async (...args) => {
            replied = true;
            return originalSendActivity(...args);
        };

        let invokeResponse;
        try {
            invokeResponse = await bot.run(context);
        } catch (err) {
            console.error('[bot] run error', err);
            if (context.activity.type === 'invoke') {
                invokeResponse = { status: 200, body: { composeExtension: { type: 'result', attachmentLayout: 'list', attachments: [] } } };
            } else {
                await context.sendActivity('The agent encountered an error handling your message.');
            }
        }

        if (context.activity.type === 'invoke') {
            if (invokeResponse) {
                context.turnState.set('invokeResponse', invokeResponse);
            } else {
                context.turnState.set('invokeResponse', { status: 200, body: { composeExtension: { type: 'result', attachmentLayout: 'list', attachments: [] } } });
            }
        } else if (context.activity.type === 'message' && !replied) {
            await context.sendActivity('Echo active (diagnostic fallback)');
        }

        await conversationState.saveChanges(context, false);
    });
});