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
const VoteCardReaderCardTemplate = require('../../resources/VoteCard.json');

class TeamsConversationBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.state = {
            IsAnonymousUser: false
        };
        this.onMessage(async (context, next) => {
            TurnContext.removeRecipientMention(context.activity);
            if (context.activity.text !== undefined) {
                const text = context.activity.text.trim().toLocaleLowerCase();
                if (text != null) {
                    if (text.includes('vote')) {
                        await context.sendActivity({ attachments: [this.SendVoteCardAsync()] });
                    }
                    else if (text.includes('createconversation')) {
                        await this.CreateConversationWithUsersAsync(context);
                    }
                    await next();
                }
            } else {
                await this.getSingleMember(context);
            }
        });

        // Invoked when bot (like a user) are added to the conversation.
        this.onTeamsMembersAddedEvent(async (membersAdded, teamInfo, context, next) => {
            if (membersAdded[0].userRole != "anonymous" && membersAdded[0].id !== context.activity.recipient.id && context.activity.conversation.conversationType !== 'personal') {
                await context.sendActivity(
                    `Welcome to the team ${membersAdded[0].givenName} ${membersAdded[0].surname}`
                );
            }
            // User role 'anonymous' indicates that newly added member is an anonymous user.
            else if (membersAdded[0].userRole == "anonymous" && membersAdded[0].id !== context.activity.recipient.id && context.activity.conversation.conversationType !== 'personal') {
                await context.sendActivity(
                    `Welcome anonymous user to the team.`
                );
            }
            await next();
        });

        // Invoked when bot (like a user) are removed to the conversation.
        this.onTeamsMembersRemovedEvent(async (membersAdded, teamInfo, context, next) => {
            // If AadObjectId property is null, it means it's an anonymous user otherwise normal user.
            if (membersAdded[0].aadObjectId == null) {
                await context.sendActivity(
                    `The anonymous user was removed from the teams meeting.`
                );
            }
            else {
                await context.sendActivity(
                    `The user was removed from the teams meeting.`
                );
            }
            await next();
        });
    }

        SendVoteCardAsync() {
            return CardFactory.adaptiveCard(VoteCardReaderCardTemplate);
        }

        async getSingleMember(context) {
            try {
                const member = await TeamsInfo.getMember(
                    context,
                    context.activity.from.id
                );
                const message = MessageFactory.text(`${member.name} voted successfully.`);
                await context.sendActivity(message);
            }
            catch (e) {
                if (e.code === 'MemberNotFoundInConversation') {
                    return context.sendActivity(MessageFactory.text('Member not found.'));
                } else {
                    throw e;
                }
            }
        }

        async CreateConversationWithUsersAsync(context) {

            const members = await this.GetPagedMembers(context);

            await Promise.all(members.map(async (member) => {
                const message = MessageFactory.text(
                    `Hello ${ member.givenName } ${ member.surname }. I'm a Teams conversation bot.`
                );

                const convoParams = {
                    members: [member],
                    tenantId: context.activity.channelData.tenant.id,
                    activity: context.activity
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
                                await context.sendActivity(message);
                            });
                    });
            }));

            await context.sendActivity(MessageFactory.text('All messages have been sent.'));
        }

        async GetPagedMembers(context) {
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

}

module.exports.TeamsConversationBot = TeamsConversationBot;