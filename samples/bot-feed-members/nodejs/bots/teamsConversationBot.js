// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    ActionTypes,
    CardFactory,
    MessageFactory,
    TeamsActivityHandler,
    TeamsInfo,
    TurnContext
} = require('botbuilder');

class TeamsConversationBot extends TeamsActivityHandler {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            const members = await TeamsInfo.getPagedMembers(context);
            TurnContext.removeRecipientMention(context.activity);

            const text = context.activity.text.trim().toLocaleLowerCase();
            if (text.includes('list')) {
                await this.listMembersAsync(context, members);
            } else {
                await this.cardActivityAsync(context, false);
            }
            await next();
        });

        this.onMembersAdded(async (context, next) => {
            const memberId = context.activity.membersAdded[0].aadObjectId;
            const member = await TeamsInfo.getMember(context, memberId);
        
            // Create the Adaptive Card with member details
            const memberCard = {
                type: "AdaptiveCard",
                body: [
                    {
                        type: "TextBlock",
                        text: `Welcome to the team, ${member.name}!`,
                        weight: "Bolder",
                        size: "ExtraLarge"  // Increased size
                    },
                    {
                        type: "TextBlock",
                        text: "Here are your details:",
                        weight: "Bolder",
                        size: "Large",  // Increased size
                        spacing: "Large"  // Added spacing
                    },
                    {
                        type: "FactSet",
                        facts: [
                            { title: "Name:", value: member.name },
                            { title: "Email:", value: member.email },
                            { title: "Given Name:", value: member.givenName },
                            { title: "Surname:", value: member.surname },
                            { title: "User Principal Name:", value: member.userPrincipalName }
                        ],
                        spacing: "ExtraLarge"  // Added spacing for more visual space
                    }
                ],
                actions: [],
                $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
                version: "1.4"
            };
        
            // Send the Adaptive Card
            await context.sendActivity({
                attachments: [CardFactory.adaptiveCard(memberCard)]
            });
        
            await next();
        });
        
    }

    async listMembersAsync(context, members) {
       // Construct Adaptive Card JSON
        const adaptiveCardJson = {
            type: 'AdaptiveCard',
            body: [
                {
                    type: 'TextBlock',
                    text: 'List of Members',
                    weight: 'Bolder',
                    size: 'Medium'
                },
                {
                    type: 'Container',
                    items: members.members.map(item => ({
                        type: 'TextBlock',
                        text: `- ${item.name}`,
                        wrap: true
                    }))
                }
            ],
            $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
            version: '1.4'
        };

        // Create an Activity object with the Adaptive Card attachment
        const reply = {
            type: 'message',
            text: 'Here is the list of members in group :',
            attachments: [CardFactory.adaptiveCard(adaptiveCardJson)]
        };

        // Send the Activity
        await context.sendActivity(reply);
    }

    async cardActivityAsync(context) {
        const cardActions = [
            {
                type: ActionTypes.MessageBack,
                title: 'List all members',
                value: null,
                text: 'List'
            }
        ];
        const card = CardFactory.heroCard(
            'Welcome card',
            '',
            null,
            cardActions
        );
        await context.sendActivity(MessageFactory.attachment(card));
    }
}
module.exports.TeamsConversationBot = TeamsConversationBot;