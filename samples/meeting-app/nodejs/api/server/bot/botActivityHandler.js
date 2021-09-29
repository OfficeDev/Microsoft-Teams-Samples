// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { TeamsActivityHandler, MessageFactory, TurnContext, TeamsInfo } = require('botbuilder');
const conversationReferences = {};
const conversationDataReferences = {};

class BotActivityHandler extends TeamsActivityHandler {
    constructor() {
        super();

        // Called when there is conversation update event.
        this.onConversationUpdate(async (context, next) => {

            // Calling method to set conversation reference.
            this.addConversationReference(context.activity);

            // Calling method to set conversation data reference that has roster information.
            this.addConversationDataReference(context);
            await next();
        });

        // Called when the bot is added to a team.
        this.onMembersAdded(async (context, next) => {
            var welcomeText = "Hello and welcome!";
            await context.sendActivity(MessageFactory.text(welcomeText));

            // Calling method to set conversation reference.
            this.addConversationReference(context.activity);

            // Calling method to set conversation data reference that has roster information.
            this.addConversationDataReference(context);
            await next();
        });

        // Activity called when there's a message in channel
        this.onMessage(async (context, next) => {

            // Calling method to set conversation reference.
            this.addConversationReference(context.activity);

            // Calling method to set conversation data reference that has roster information.
            this.addConversationDataReference(context);
            var replyText = context.activity.text;
            await context.sendActivity(MessageFactory.text(`You sent '${replyText}'`));
            await next();
        });
    }

    // Method to set conversation reference.
    addConversationReference(activity) {
        const conversationReference = TurnContext.getConversationReference(activity);
        conversationReferences[conversationReference.conversation.id] = conversationReference;
    }

    // Method to set conversation data reference that has roster information.
    async addConversationDataReference(context) {
        var members = await TeamsInfo.getMembers(context);
        conversationDataReferences["members"] = members;
    }
}

module.exports.BotActivityHandler = BotActivityHandler;

// Exporting conversationReferences to be used for proactive messaging
exports.ConversationRef = conversationReferences;

// Exporting conversationDataReferences to use roster information.
exports.ConversationDataRef = conversationDataReferences;
