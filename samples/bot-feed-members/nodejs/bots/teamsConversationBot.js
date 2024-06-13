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
            // Code to get details of all members in group chat
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
                const members = await TeamsInfo.getPagedMembers(context);
                var memberId = context.activity.membersAdded[0].aadObjectId;
                var member = await TeamsInfo.getMember(context, memberId);
                await context.sendActivity(`Welcome to the team :${member.name}`);
                await this.listMembersAsync(context, members);
                
            await next();
        });
    }

    async onInstallationUpdateActivity(context) {
        if (context.activity.conversation.conversationType === 'channel') {
            await context.sendActivity(MessageFactory.text(`Welcome to Microsoft Teams type list by @ mention the bot to get list of all members.`));
        } else {
            await context.sendActivity(MessageFactory.text('Welcome to Microsoft Teams type list by @ mention the bot to get list of all members.'));
        }
    }

    async listMembersAsync(context, members) {
       // Construct Adaptive Card JSON
       const adaptiveCardJson = {
        type: 'AdaptiveCard',
        body: [
            {
                type: 'TextBlock',
                text: 'List Example',
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
        text: 'Here is the list of members in group:',
        attachments: [CardFactory.adaptiveCard(adaptiveCardJson)]
    };

    // Send the Activity
    await context.sendActivity(reply);
    }

    async cardActivityAsync(context, isUpdate) {
        const cardActions = [
            {
                type: ActionTypes.MessageBack,
                title: 'List all members',
                value: null,
                text: 'List'
            }
        ];

        if (isUpdate) {
            await this.sendUpdateCard(context, cardActions);
        } else {
            await this.sendWelcomeCard(context, cardActions);
        }
    }

    async sendUpdateCard(context, cardActions) {
        const data = context.activity.value;
        data.count += 1;
        cardActions.push({
            type: ActionTypes.MessageBack,
            title: 'Update Card',
            value: data,
            text: 'UpdateCardAction'
        });
        const card = CardFactory.heroCard(
            'Updated card',
            `Update count: ${data.count}`,
            null,
            cardActions
        );
        card.id = context.activity.replyToId;
        const message = MessageFactory.attachment(card);
        message.id = context.activity.replyToId;
        await context.updateActivity(message);
    }

    async sendWelcomeCard(context, cardActions) {
        const initialValue = {
            count: 0
        };
        cardActions.push({
            type: ActionTypes.MessageBack,
            title: 'Update Card',
            value: initialValue,
            text: 'UpdateCardAction'
        });
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