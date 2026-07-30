// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import path from "path";
import restify from "restify";
import { EchoBot } from "./bot.js";
import tabs from "./tabs.js";
import MessageExtension from "./message-extension.js";
import {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication,
    MemoryStorage,
    ConversationState,
    ActivityTypes,
} from "botbuilder";

// Load environment variables from .env (at project root)
const ENV_FILE = path.join(__dirname, "../.env");
require("dotenv").config({ path: ENV_FILE });

// Configure authentication using environment variables
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

// Define storage and conversation state
const memoryStorage = new MemoryStorage();
const conversationState = new ConversationState(memoryStorage);

// Create CloudAdapter with authentication
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Global error handler for the adapter
adapter.onTurnError = async (context, error) => {
    console.error(`[onTurnError] Unhandled error: ${error}`);
    console.error(error.stack);

    try {
        // Clear state in case it is corrupted
        await conversationState.delete(context);
    } catch (err) {
        console.error(`Error clearing conversation state: ${err}`);
    }

    // Send generic error message to user
    await context.sendActivity("The bot encountered an error.");
    await context.sendActivity("Please try again later.");
};

// Create HTTP server with Restify
const server = restify.createServer({
    formatters: {
        "text/html": (req, res, body) => body, // return HTML as-is
    },
});

// Enable body parser (required for bot to process messages)
server.use(restify.plugins.bodyParser());

// Serve static files (for tabs, UI assets, etc.)
server.get(
    "/*",
    restify.plugins.serveStatic({
        directory: path.join(__dirname, "static"),
    })
);

// Start the server
server.listen(process.env.port || process.env.PORT || 3978, () => {
    console.log(`${server.name} listening to ${server.url}`);
});

// Initialize bot and message extension
tabs(server);
const bot = new EchoBot();
const messageExtension = new MessageExtension();

// Handle incoming requests from Teams
server.post("/api/messages", async (req, res) => {
    console.log("Received request at /api/messages");
    await adapter.process(req, res, async (context) => {
        console.log("Inside adapter.process");

        // If activity is an invoke, handle message extension
        if (context.activity.type === ActivityTypes.Invoke) {
            await messageExtension.run(context);
        } else {
            // Otherwise, pass to bot logic
            await bot.run(context);
        }

        // Save state changes
        await conversationState.saveChanges(context, false);
    });
});
