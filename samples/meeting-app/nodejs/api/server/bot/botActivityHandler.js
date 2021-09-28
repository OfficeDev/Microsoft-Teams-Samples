// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, MessageFactory, TurnContext } = require('botbuilder');

const conversationReferences = {};

class BotActivityHandler extends TeamsActivityHandler {
    constructor(conversationReferences) {
        super();

        // Dependency injected dictionary for storing ConversationReference objects used to proactively message users.
        this.conversationReferences = conversationReferences !== undefined ? conversationReferences: {};

        // Dependency injected dictionary for storing Conversation Data that has roster and note details.
        //this.conversationDataReferences = conversationDataReferences;

        this.onConversationUpdate(async (context, next) => {
            this.addConversationReference(context.activity);

            await next();
        });

        // Called when the bot is added to a team.
        this.onMembersAdded(async (context, next) => {
            var welcomeText = "Hello and welcome!";
            await context.sendActivity(MessageFactory.text(welcomeText));
            await next();
        });

        // Activity called when there's a message in channel
        this.onMessage(async (context, next) => {
            this.addConversationReference(context.activity);

            var replyText = context.activity.text;
            await context.sendActivity(MessageFactory.text(`You sent '${replyText}'`));
            await next();
        });
    }

    addConversationReference(activity) {
        const conversationReference = TurnContext.getConversationReference(activity);
        conversationReferences[conversationReference.conversation.id] = conversationReference;
    }

    // addConversationDataReference(activity) {
    //     const conversationReference = TurnContext.getConversationReference(activity);
    //     this.conversationReferences[conversationReference.conversation.id] = conversationReference;
    // }
}

module.exports.BotActivityHandler = BotActivityHandler;
exports.ConversationRef = conversationReferences;
