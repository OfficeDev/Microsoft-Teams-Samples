// <copyright file="teamsConversationBot.js" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

const {
    ActionTypes,
    CardFactory,
    MessageFactory,
    TeamsActivityHandler,
    TeamsInfo,
    TurnContext
} = require('botbuilder');
// Read vote card JSON template
const VoteCardReaderCardTemplate = require('../../resources/VoteCard.json');

class TeamsConversationBot extends TeamsActivityHandler {
    constructor() {
        super();
        // Handle when a message is addressed to the bot.
        this.onMessage(async (context, next) => {
             // Remove bot at-mentions for teams/groupchat scope
            TurnContext.removeRecipientMention(context.activity);
            
            if (context.activity.text !== undefined) {
                const text = context.activity.text.trim().toLocaleLowerCase();
                if (text != null) {
                    if (text.includes('vote')) {
                        await context.sendActivity({ attachments: [this.SendVoteCardAsync()] });
                    }
                    else if (text.includes('createconversation')) {
                       await this.cardActivityAsync(context);
                    }
                    else if (text.includes('message')) {
                        // Create 1:1 bot conversation with users existing in the current meeting.
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
        async cardActivityAsync(context) {
            const cardActions = [
                {
                    type: ActionTypes.MessageBack,
                    title: 'Message all members',
                    value: null,
                    text: 'message'
                }
            ];

            const card = CardFactory.heroCard(
                '',
                '',
                null,
                cardActions
            );

            await context.sendActivity(MessageFactory.attachment(card));
        }
       
        // Send vote adaptive card template.
        SendVoteCardAsync() {
            return CardFactory.adaptiveCard(VoteCardReaderCardTemplate);
        }
       
        // Fetching member information and sending a vote success message
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
        // The conversation information to use to create the conversation.
        async CreateConversationWithUsersAsync(context) {

            let usersCount = 0;
            let anonymousUsersCount = 0;
            let isAnonymousUser = false;

            const members = await this.getPagedMembers(context);
                await Promise.all(members.map(async (member) => {
                const message = MessageFactory.text(
                    `Hello ${ member.givenName } ${ member.surname } I'm a Teams conversation bot that support anonymous users.`
                );

                if (member.userRole != null && member.userRole == "user") {
                    usersCount++;
                    }
                
                if (member.userRole != null && member.userRole == "anonymous") {
                    anonymousUsersCount++;
                    }   
                
                const convoParams = {
                    members: [member],
                    tenantId: context.activity.channelData.tenant.id,
                    activity: context.activity
                };
                
               try
               {
                // Creates a conversation on the specified groupchat and send file consent card on that conversation.
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
                }
                catch(Exception)
                {
                    // This condition is handling error for anonymous users because "Bot cannot create a conversation with an anonymous user".
                    if (Exception.message.includes("anonymous"))
                    {
                        isAnonymousUser = true;
                    }                    
                }
            }));
            
            if (isAnonymousUser) {
                await context.sendActivity(MessageFactory.text(`Users count: ${usersCount} <br> Anonymous users count: ${anonymousUsersCount} <br> Note: Bot cannot create a conversation with an anonymous user.`));
            }
            
            await context.sendActivity(MessageFactory.text('All messages have been sent.'));
        }
        
        // Gets a paginated list of members of a team. 
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
}

module.exports.TeamsConversationBot = TeamsConversationBot;