// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, MessageFactory, TurnContext, TeamsInfo } = require('botbuilder');

const conversationReferences = {};
const conversationDataReferences = {};
class BotActivityHandler extends TeamsActivityHandler {
    constructor() {
        super();

        this.onConversationUpdate(async (context, next) => {
            this.addConversationReference(context.activity);
            this.addConversationDataReference(context);
            await next();
        });

        // Called when the bot is added to a team.
        this.onMembersAdded(async (context, next) => {
            var welcomeText = "Hello and welcome!";
            await context.sendActivity(MessageFactory.text(welcomeText));
            this.addConversationReference(context.activity);
            this.addConversationDataReference(context);
            await next();
        });

        // Activity called when there's a message in channel
        this.onMessage(async (context, next) => {
            this.addConversationReference(context.activity);
            this.addConversationDataReference(context);
            var replyText = context.activity.text;
            await context.sendActivity(MessageFactory.text(`You sent '${replyText}'`));
            await next();
        });
    }

    addConversationReference(activity) {
        const conversationReference = TurnContext.getConversationReference(activity);
        conversationReferences[conversationReference.conversation.id] = conversationReference;
    }

    async addConversationDataReference(context) {
        var members = await TeamsInfo.getMembers(context);
        conversationDataReferences["members"] = members;
    }
}

module.exports.BotActivityHandler = BotActivityHandler;
exports.ConversationRef = conversationReferences;
exports.ConversationDataRef = conversationDataReferences;
