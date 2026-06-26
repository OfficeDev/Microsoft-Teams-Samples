// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    ActionTypes,
    CardFactory,
    MessageFactory,
    TeamsActivityHandler,
    TeamsInfo,
    TurnContext,
    ActivityTypes
} = require('botbuilder');
const ACData = require('adaptivecards-templating');
const AdaptiveCardTemplate = require('../resources/UserMentionCardTemplate.json');
const ImmersiveReaderCardTemplate = require('../resources/ImmersiveReaderCard.json');
let counter = 0;
let users = [];
let teamMemberDetails = [];

class TeamsConversationBot extends TeamsActivityHandler {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            TurnContext.removeRecipientMention(context.activity);

            const text = context.activity.text.trim().toLocaleLowerCase();
            if (text.includes('mention me')) {
                await this.mentionAdaptiveCardActivityAsync(context);
            } else if (text.includes('mention')) {
                await this.mentionActivityAsync(context);
            } else if (text.includes('update')) {
                await this.cardActivityAsync(context, true);
            } else if (text.includes('delete')) {
                await this.deleteCardActivityAsync(context);
            } else if (text.includes('aadid')) {
                await this.messageAllMembersAsync(context, true);
            } else if (text.includes('message')) {
                await this.messageAllMembersAsync(context, false);
            } else if (text.includes('who')) {
                await this.getSingleMember(context);
            } else if (text.includes('immersivereader')) {
                await this.getImmersivereaderCard(context);
            } else if (text.includes('check')) {
                await this.checkReadUserCount(context);
            } else if (text.includes('reset')) {
                await this.resetReadUserCount(context);
            } else if (text.includes('label')) {
                await this.addAILabel(context);
            } else if (text.includes('sensitivity')) {
                await this.addSensitivityLabel(context);
            } else if (text.includes('feedback')) {
                await this.addFeedbackButtons(context);
            } else if (text.includes('citation')) {
                await this.addCitations(context);
            } else if (text.includes('aitext')) {
                await this.sendAIMessage(context);
            } else {
                await this.cardActivityAsync(context, false);
            }

            await next();
        });

        this.onMembersAddedActivity(async (context, next) => {
            await Promise.all((context.activity.membersAdded || []).map(async (member) => {
                if (member.id !== context.activity.recipient.id && context.activity.conversation.conversationType !== 'personal') {
                    await context.sendActivity(
                        `Welcome to the team ${member.givenName} ${member.surname}`
                    );
                }
            }));

            await next();
        });

        this.onReactionsAdded(async (context) => {
            await Promise.all((context.activity.reactionsAdded || []).map(async (reaction) => {
                const newReaction = `You reacted with '${reaction.type}' to the following message: '${context.activity.replyToId}'`;
                await context.sendActivity(newReaction);
                // Save information about the sent message and its ID (resourceResponse.id).
            }));
        });

        this.onReactionsRemoved(async (context) => {
            await Promise.all((context.activity.reactionsRemoved || []).map(async (reaction) => {
                const newReaction = `You removed the reaction '${reaction.type}' from the message: '${context.activity.replyToId}'`;
                await context.sendActivity(newReaction);
                // Save information about the sent message and its ID (resourceResponse.id).
            }));
        });

        // Invoked when user read the message sent by bot in personal scope.
        this.onTeamsReadReceiptEvent(async (readReceiptInfo, turnContext, next) => {
            const memberDetails = teamMemberDetails.find(member => member.aadObjectId === turnContext._activity.from.aadObjectId);
            if (memberDetails && readReceiptInfo.isMessageRead(memberDetails.messageId)) {
                users.push(memberDetails.name);
                counter++;
                teamMemberDetails = teamMemberDetails.filter(member => member.aadObjectId !== turnContext._activity.from.aadObjectId);
            }
        });

        // Invoked when a message sent by user is updated in chat.
        this.onTeamsMessageEditEvent(async (context, next) => {
            const editedMessage = context.activity.text;
            await context.sendActivity(`The edited message is "${editedMessage}"`);
            await next();
        });

        // Invoked when a message sent by user is undeleted in chat.
        this.onTeamsMessageUndeleteEvent(async (context, next) => {
            const undeletedMessage = context.activity.text;
            await context.sendActivity(`Previously the message was deleted. After undeleting, the message is now: "${undeletedMessage}"`);
            await next();
        });

        // Invoked when a message sent by user is soft deleted in chat.
        this.onTeamsMessageSoftDeleteEvent(async (context, next) => {
            await context.sendActivity('Message is soft deleted');
            await next();
        });
    }

    async sendAIMessage(context) {
        await context.sendActivity({
            type: ActivityTypes.Message,
            text: 'Hey I\'m a friendly AI bot. This message is generated via AI [1]',
            channelData: {
                feedbackLoopEnabled: true
            },
            entities: [
                {
                    type: 'https://schema.org/Message',
                    '@type': 'Message',
                    '@context': 'https://schema.org',
                    usageInfo: {
                        '@type': 'CreativeWork',
                        '@id': 'sensitivity1'
                    },
                    additionalType: ['AIGeneratedContent'],
                    citation: [
                        {
                            '@type': 'Claim',
                            position: 1,
                            appearance: {
                                '@type': 'DigitalDocument',
                                name: 'Some secret citation',
                                url: 'https://example.com/claim-1',
                                abstract: 'Excerpt',
                                encodingFormat: 'docx',
                                keywords: ['Keyword1 - 1', 'Keyword1 - 2', 'Keyword1 - 3'],
                                usageInfo: {
                                    '@type': 'CreativeWork',
                                    '@id': 'sensitivity1',
                                    name: 'Sensitivity title',
                                    description: 'Sensitivity description'
                                }
                            }
                        }
                    ]
                }
            ]
        });
    }

    async addAILabel(turnContext) {
        await turnContext.sendActivity({
            type: ActivityTypes.Message,
            text: 'Hey I\'m a friendly AI bot. This message is generated via AI',
            entities: [
                {
                    type: 'https://schema.org/Message',
                    '@type': 'Message',
                    '@context': 'https://schema.org',
                    additionalType: ['AIGeneratedContent']
                }
            ]
        });
    }

    async addSensitivityLabel(turnContext) {
        await turnContext.sendActivity({
            type: ActivityTypes.Message,
            text: 'This is an example for sensitivity label that help users identify the confidentiality of a message',
            entities: [
                {
                    type: 'https://schema.org/Message',
                    '@type': 'Message',
                    '@context': 'https://schema.org',
                    usageInfo: {
                        '@type': 'CreativeWork',
                        description: 'Please be mindful of sharing outside of your team',
                        name: 'Confidential \\ Contoso FTE'
                    }
                }
            ]
        });
    }

    async addFeedbackButtons(turnContext) {
        await turnContext.sendActivity({
            type: ActivityTypes.Message,
            text: 'This is an example for Feedback buttons that helps to provide feedback for a bot message',
            channelData: {
                feedbackLoopEnabled: true
            }
        });
    }

    async addCitations(turnContext) {
        await turnContext.sendActivity({
            type: ActivityTypes.Message,
            text: 'Hey I\'m a friendly AI bot. This message is generated through AI [1]',
            entities: [
                {
                    type: 'https://schema.org/Message',
                    '@type': 'Message',
                    '@context': 'https://schema.org',
                    citation: [
                        {
                            '@type': 'Claim',
                            position: 1,
                            appearance: {
                                '@type': 'DigitalDocument',
                                name: 'AI bot',
                                url: 'https://example.com/claim-1',
                                abstract: 'Excerpt description',
                                text: '{"type":"AdaptiveCard","$schema":"http://adaptivecards.io/schemas/adaptive-card.json","version":"1.6","body":[{"type":"TextBlock","text":"Adaptive Card text"}]}',
                                keywords: ['keyword 1', 'keyword 2', 'keyword 3'],
                                encodingFormat: 'application/vnd.microsoft.card.adaptive',
                                usageInfo: {
                                    '@type': 'CreativeWork',
                                    name: 'Confidential \\ Contoso FTE',
                                    description: 'Only accessible to Contoso FTE'
                                },
                                image: {
                                    '@type': 'ImageObject',
                                    name: 'Microsoft Word'
                                }
                            }
                        }
                    ]
                }
            ]
        });
    }

    // Checks the count of members who have read the message sent by MessageAllMembers command.
    async checkReadUserCount(turnContext) {
        if (users.length > 0) {
            const userList = Array.from(users).join(", ");
            await turnContext.sendActivity(`Number of members read the message: ${counter}\n\nMembers: ${userList}`);
        } else {
            await turnContext.sendActivity('Read count is zero. Please make sure to send a message to all members first to check the count of members who have read your message.');
        }
    }

    async onInvokeActivity(context) {
        try {
            switch (context.activity.name) {
                case 'message/submitAction':
                    return await context.sendActivity(
                        `Provided reaction : ${context.activity.value.actionValue.reaction}<br> Feedback : ${JSON.parse(context.activity.value.actionValue.feedback).feedbackText}`
                    );
                default:
                    return {
                        status: 200,
                        body: `Unknown invoke activity handled as default - ${context.activity.name}`
                    };
            }
        } catch (err) {
            console.error(`Error in onInvokeActivity: ${err}`);
            return {
                status: 500,
                body: `Invoke activity received - ${context.activity.name}`
            };
        }
    }

    // Resets the check count of members who have read the message sent by MessageAllMembers command.
    async resetReadUserCount(turnContext) {
        teamMemberDetails = [];
        counter = 0;
        users = [];
    }

    async cardActivityAsync(context, isUpdate) {
        const cardActions = [
            {
                type: ActionTypes.MessageBack,
                title: 'Message all members',
                value: null,
                text: 'MessageAllMembers'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Message all members using AADId',
                value: null,
                text: 'MessageAllMembersUsingAadId'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Who am I?',
                value: null,
                text: 'whoami'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Find me in Adaptive Card',
                value: null,
                text: 'mention me'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Delete card',
                value: null,
                text: 'Delete'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Send Immersive Reader Card',
                value: null,
                text: 'ImmersiveReader'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Check read count',
                value: null,
                text: 'check'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Reset read count',
                value: null,
                text: 'reset'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'AI label',
                value: null,
                text: 'label'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Sensitivity label',
                value: null,
                text: 'sensitivity'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Feedback buttons',
                value: null,
                text: 'feedback'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Citations',
                value: null,
                text: 'citation'
            },
            {
                type: ActionTypes.MessageBack,
                title: 'Send AI message',
                value: null,
                text: 'sendAItext'
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

    async getSingleMember(context) {
        try {
            const member = await TeamsInfo.getMember(
                context,
                context.activity.from.id
            );
            const message = MessageFactory.text(`You are: ${member.name}`);
            await context.sendActivity(message);
        } catch (e) {
            if (e.code === 'MemberNotFoundInConversation') {
                return context.sendActivity(MessageFactory.text('Member not found.'));
            } else {
                throw e;
            }
        }
    }

    async mentionAdaptiveCardActivityAsync(context) {
        let member;
        try {
            member = await TeamsInfo.getMember(
                context,
                context.activity.from.id
            );
        } catch (e) {
            if (e.code === 'MemberNotFoundInConversation') {
                return context.sendActivity(MessageFactory.text('Member not found.'));
            } else {
                throw e;
            }
        }

        const template = new ACData.Template(AdaptiveCardTemplate);
        const memberData = {
            userName: member.name,
            userUPN: member.userPrincipalName,
            userAAD: member.aadObjectId
        };

        const adaptiveCard = template.expand({
            $root: memberData
        });

        await context.sendActivity({
            attachments: [CardFactory.adaptiveCard(adaptiveCard)]
        });
    }

    async mentionActivityAsync(context) {
        const mention = {
            mentioned: context.activity.from,
            text: `<at>${context.activity.from.name}</at>`,
            type: 'mention'
        };

        const replyActivity = MessageFactory.text(`Hi ${mention.text}`);
        replyActivity.entities = [mention];
        await context.sendActivity(replyActivity);
    }

    async getImmersivereaderCard(context) {
        await context.sendActivity({ attachments: [CardFactory.adaptiveCard(ImmersiveReaderCardTemplate)] });
    }

    async deleteCardActivityAsync(context) {
        await context.deleteActivity(context.activity.replyToId);
    }

    async messageAllMembersAsync(context, isAadId) {
        const members = await this.getPagedMembers(context);
        await this.resetReadUserCount(context);

        await Promise.all(members.map(async (member) => {
            const message = MessageFactory.text(
                `Hello ${member.givenName} ${member.surname}. I'm a Teams conversation bot.`
            );

            const convoParams = {
                members: [isAadId ? { id: member.aadObjectId } : { id: member.id }],
                isGroup: false,
                bot: context.activity.recipient,
                tenantId: context.activity.conversation.tenantId
            };

            await context.adapter.createConversationAsync(
                process.env.MicrosoftAppId,
                context.activity.channelId,
                context.activity.serviceUrl,
                null,
                convoParams,
                async (context) => {
                    const ref = TurnContext.getConversationReference(context.activity);

                    await context.adapter.continueConversationAsync(
                        process.env.MicrosoftAppId,
                        ref,
                        async (context) => {
                            const messageId = await context.sendActivity(message);
                            member.messageId = messageId.id;
                            teamMemberDetails.push(member);
                        });
                });
        }));

        await context.sendActivity(MessageFactory.text('All messages have been sent.'));
    }

    async getPagedMembers(context) {
        let continuationToken;
        const members = [];

        do {
            const page = await TeamsInfo.getPagedMembers(
                context,
                100,
                continuationToken
            );

            continuationToken = page.continuationToken;

            members.push(...page.members);
        } while (continuationToken !== undefined);

        return members;
    }

    async onTeamsChannelCreated(context) {
        const card = CardFactory.heroCard(
            'Channel Created',
            `${context.activity.channelData.channel.name} is new the Channel created`
        );
        const message = MessageFactory.attachment(card);
        await context.sendActivity(message);
    }

    async onTeamsChannelRenamed(context) {
        const card = CardFactory.heroCard(
            'Channel Renamed',
            `${context.activity.channelData.channel.name} is the new Channel name`
        );
        const message = MessageFactory.attachment(card);
        await context.sendActivity(message);
    }

    async onTeamsChannelDeleted(context) {
        const card = CardFactory.heroCard(
            'Channel Deleted',
            `${context.activity.channelData.channel.name} is deleted`
        );
        const message = MessageFactory.attachment(card);
        await context.sendActivity(message);
    }

    async onTeamsChannelRestored(context) {
        const card = CardFactory.heroCard(
            'Channel Restored',
            `${context.activity.channelData.channel.name} is the Channel restored`
        );
        const message = MessageFactory.attachment(card);
        await context.sendActivity(message);
    }

    async onTeamsTeamRenamed(context) {
        const card = CardFactory.heroCard(
            'Team Renamed',
            `${context.activity.channelData.team.name} is the new Team name`
        );
        const message = MessageFactory.attachment(card);
        await context.sendActivity(message);
    }
}

module.exports.TeamsConversationBot = TeamsConversationBot;