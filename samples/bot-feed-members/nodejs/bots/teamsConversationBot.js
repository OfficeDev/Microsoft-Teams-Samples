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

// The TeamsConversationBot class extends TeamsActivityHandler to handle Teams-specific activities.
class TeamsConversationBot extends TeamsActivityHandler {
    constructor() {
        super();

        // Handle messages sent to the bot.
        this.onMessage(async (context, next) => {
            // Get a paginated list of members in the current Teams conversation.
            const members = await TeamsInfo.getPagedMembers(context);
            
            // Remove the bot's mention from the message text to clean up the user's input.
            TurnContext.removeRecipientMention(context.activity);

            // Normalize the message text for case-insensitive comparison.
            const text = context.activity.text.trim().toLocaleLowerCase();
            
            // If the message contains 'list', list the members. Otherwise, send a welcome card.
            if (text.includes('list')) {
                await this.listMembersAsync(context, members);
            } else {
                await this.cardActivityAsync(context, false);
            }
            // Call the next middleware in the pipeline.
            await next();
        });

        // Handle new members added to the conversation.
        this.onMembersAdded(async (context, next) => {
            // Get the ID of the first member added.
            const memberId = context.activity.membersAdded[0].aadObjectId;
            
            // Fetch detailed information about the new member.
            const member = await TeamsInfo.getMember(context, memberId);
        
            // Create an Adaptive Card to welcome the new member and display their details.
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
                            { title: "User Principal Name:", value: member.userPrincipalName },
                            { title: "Tenant Id:", value: member.tenantId }
                        ],
                        spacing: "ExtraLarge"  // Added spacing for more visual space
                    }
                ],
                actions: [],
                $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
                version: "1.4"
            };
        
            // Send the Adaptive Card as an activity to the user.
            await context.sendActivity({
                attachments: [CardFactory.adaptiveCard(memberCard)]
            });
        
            // Call the next middleware in the pipeline.
            await next();
        });
    }

    // Lists all members in the current Teams conversation.
    async listMembersAsync(context, members) {
        // Construct the Adaptive Card JSON structure with the list of members.
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

        // Create an Activity object containing the Adaptive Card as an attachment.
        const reply = {
            type: 'message',
            text: 'Here is the list of members in group:',
            attachments: [CardFactory.adaptiveCard(adaptiveCardJson)]
        };

        // Send the Activity with the list of members to the user.
        await context.sendActivity(reply);
    }

    // Sends a welcome card with an option to list all members.
    async cardActivityAsync(context) {
        // Define the action button to list all members.
        const cardActions = [
            {
                type: ActionTypes.MessageBack,
                title: 'List all members',
                value: null,
                text: 'List'
            }
        ];
        
        // Create a Hero Card with the defined action button.
        const card = CardFactory.heroCard(
            'Welcome card',
            '',
            null,
            cardActions
        );

        // Send the Hero Card as an activity to the user.
        await context.sendActivity(MessageFactory.attachment(card));
    }
}

// Export the TeamsConversationBot class for use in other files.
module.exports.TeamsConversationBot = TeamsConversationBot;
