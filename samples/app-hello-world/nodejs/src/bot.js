// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import {
    TeamsActivityHandler,
    BotFrameworkAdapter,
    MemoryStorage,
    ConversationState,
    TurnContext,
} from 'botbuilder';
import config from 'config';

// Create adapter and configure it with app credentials from config.
// See https://aka.ms/about-bot-adapter for more information.
export const adapter = new BotFrameworkAdapter({
    appId: config.get('bot.appId'),
    appPassword: config.get('bot.appPassword'),
});

// Error handler to catch errors during bot interactions.
adapter.onTurnError = async (context, error) => {
    const errorMsg = error.message || 'Oops. Something went wrong!';
    // Log error (Consider logging to Azure Application Insights in production)
    console.error(`[onTurnError] unhandled error: ${error}`);

    // Clear any state that may have been corrupted during the error.
    await conversationState.delete(context);

    // Send error message to user.
    await context.sendActivity(errorMsg);

    // For local debugging, uncomment the line below to send detailed error message.
    // await context.sendActivity(`Sorry, something went wrong. Exception: ${errorMsg}`);
};

// Define in-memory storage for the bot's conversation state.
const memoryStorage = new MemoryStorage();

// Create a conversation state with the in-memory storage provider.
const conversationState = new ConversationState(memoryStorage);

// EchoBot class that inherits from TeamsActivityHandler.
export class EchoBot extends TeamsActivityHandler {
    constructor() {
        super();

        // Handles incoming messages and echoes them back.
        this.onMessage(async (context, next) => {
            // Remove recipient mention to avoid bot calling itself.
            TurnContext.removeRecipientMention(context.activity);

            // Process the incoming message, convert it to lowercase, and send a response.
            const text = context.activity.text.trim().toLowerCase();
            await context.sendActivity(`You said: ${text}`);

            // Proceed to the next middleware, if any.
            await next();
        });
    }
}
