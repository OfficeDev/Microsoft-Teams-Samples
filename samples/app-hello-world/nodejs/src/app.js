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
import restify from "restify";
import dotenv from "dotenv";
import { fileURLToPath } from "url";
import { EchoBot } from "./bot.js";
import tabs from "./tabs.js";
import MessageExtension from "./message-extension.js";

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

// Create and configure the HTTP server using Restify
const server = restify.createServer({
    formatters: {
        "text/html": (req, res, body) => body, // Return HTML responses as-is
    },
});

// Enable body parsing middleware
server.use(restify.plugins.bodyParser());

// Serve static files such as tab pages or assets
server.get(
    "/*",
    restify.plugins.serveStatic({
        directory: path.join(__dirname, "static"),
    })
);

// Start the web server
server.listen(process.env.PORT || 3978, () => {
    console.log(`${server.name} listening to ${server.url}`);
});

// Initialize bot logic and message extension
const bot = new EchoBot();
const messageExtension = new MessageExtension();

// Register tab routes
tabs(server);

// Handle incoming Teams or Copilot activities
server.post("/api/messages", async (req, res) => {
    console.log("Received request at /api/messages");

    await adapter.process(req, res, async (context) => {
        console.log("Processing activity:", context.activity.type);

        if (context.activity.type === "invoke") {
            // Handle message extension invoke
            await messageExtension.run(context);
        } else {
            // Handle normal conversation messages
            await bot.run(context);
        }

        // Save state after processing the turn
        await conversationState.saveChanges(context, false);
    });
});
