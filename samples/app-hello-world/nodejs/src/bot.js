// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { ActivityHandler } from "@microsoft/agents-hosting";

// Helper function to remove bot mentions from a message
function removeRecipientMention(activity) {
    const text = activity.text || "";
    const recipientId = activity.recipient?.id;

    if (!recipientId) return text;

    // Remove <at>mention</at> for the bot
    return text.replace(/<at>.*?<\/at>/g, "").trim();
}

// EchoBot: simple bot that replies with "You said: <message>"
export class EchoBot extends ActivityHandler {
    constructor() {
        super();

        // Handle incoming messages
        this.onMessage(async (context, next) => {
            // Remove bot mention manually
            const text = removeRecipientMention(context.activity).toLowerCase();

            // Echo back the same text
            await context.sendActivity(`You said: ${text}`);

            // Continue to the next middleware
            await next();
        });
    }
}
