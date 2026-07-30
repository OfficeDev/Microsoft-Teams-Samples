// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { TeamsActivityHandler, TurnContext } from "botbuilder";

// EchoBot: simple bot that replies with "You said: <message>"
export class EchoBot extends TeamsActivityHandler {
    constructor() {
        super();

        // Handle incoming messages
        this.onMessage(async (context, next) => {
            // Remove mention so the bot does not echo its own @mention
            TurnContext.removeRecipientMention(context.activity);

            // Get the incoming message text safely
            const text = context.activity.text?.trim().toLowerCase() || "";

            // Echo back the same text
            await context.sendActivity(`You said: ${text}`);

            // Continue to the next middleware
            await next();
        });
    }
}
