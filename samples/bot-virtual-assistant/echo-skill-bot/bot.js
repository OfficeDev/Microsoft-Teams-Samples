// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityHandler, ActivityTypes, EndOfConversationCodes } = require('botbuilder');

// Define the EchoBot class, which extends the ActivityHandler class from botbuilder.
class EchoBot extends ActivityHandler {
    constructor() {
        // Call the constructor of the superclass (ActivityHandler).
        super();

        // Set up an onMessage handler to handle incoming messages.
        this.onMessage(async (context, next) => {
            // Echo back the received message text.
            await context.sendActivity(`Echo bot: ${ context.activity.text }`);

            // Send an EndOfConversation activity to indicate completion
            await context.sendActivity({
                type: ActivityTypes.EndOfConversation,
                code: EndOfConversationCodes.CompletedSuccessfully
            });

            // Call the next handler in the pipeline.
            await next();
        });
    }
}

// Export the EchoBot class so it can be used in other files.
module.exports.EchoBot = EchoBot;
