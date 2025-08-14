// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler } = require('botbuilder');

// Initializing conversationID and serviceUrl
let ConversationID = "";
let serviceUrl = "";

class SidePanelBot extends TeamsActivityHandler {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            ConversationID = context._activity.conversation.id;
            exports.ConversationID = ConversationID;
            serviceUrl = context._activity.serviceUrl;
            exports.serviceUrl = serviceUrl;
            await context.sendActivity("Welcome to SidePanel Application!");
        });

        this.onConversationUpdate(async (context, next) => {
            ConversationID = context._activity.conversation.id;
            exports.ConversationID = ConversationID;
            serviceUrl = context._activity.serviceUrl;
            exports.serviceUrl = serviceUrl;
        });

        this.onMembersAddedActivity(async (context, next) => {
            context._activity.membersAdded.forEach(async (teamMember) => {
                if (teamMember.id !== context._activity.recipient.id) {
                    await context.sendActivity(`Welcome to the team ${teamMember.givenName} ${teamMember.surname}`);
                }
            });
            await next();
        });
    }
}

module.exports.SidePanelBot = SidePanelBot;
