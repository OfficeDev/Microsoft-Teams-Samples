// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { TeamsActivityHandler, MessageFactory, TurnContext } from 'botbuilder';

export class BotActivityHandler extends TeamsActivityHandler {
    constructor() {
        super();

        const sampleDescription = "With this sample your bot can receive user messages across standard channels in a team without being @mentioned";
        const option = "Type 1 to know about the permissions required,  Type 2 for documentation link";
        const permissionRequired = "This capability is enabled by specifying the ChannelMessage.Read.Group permission in the manifest of an RSC enabled Teams app";
        const docLink = "https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/channel-messages-with-rsc";

        // Activity called when there's a message in channel
        this.onMessage(async (context: TurnContext, next) => {
            const replyText = context.activity.text;
            if (replyText === "1") {
                await context.sendActivity(MessageFactory.text(permissionRequired));
            } else if (replyText === "2") {
                await context.sendActivity(MessageFactory.text(docLink));
            } else {
                await context.sendActivity(MessageFactory.text(sampleDescription));
                await context.sendActivity(MessageFactory.text(option));
            }
            await next();
        });

        // Called when the bot is added to a team.
        this.onMembersAdded(async (context: TurnContext, next) => {
            const welcomeText = "Hello and welcome! With this sample your bot can receive user messages across standard channels in a team without being @mentioned";
            await context.sendActivity(MessageFactory.text(welcomeText));
            await next();
        });
    }
}
