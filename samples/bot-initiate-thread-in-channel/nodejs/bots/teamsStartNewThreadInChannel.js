// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    MessageFactory,
    TeamsActivityHandler,
    TeamsInfo,
    CardFactory
} = require('botbuilder');

class TeamsStartNewThreadInChannel extends TeamsActivityHandler {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            const text = context.activity.text.trim().toLowerCase();

            if (text.includes('listchannels')) {
                await this.listTeamChannels(context);
            } else if (text.includes('threadchannel')) {
                await this.startNewThreadInChannel(context);
            } else if (text.includes('getteammember')) {
                await this.getTeamMember(context);
            } else if (text.includes('getpagedteammembers')) {
                await this.getPagedTeamMembers(context);
            } else {
                await context.sendActivity("I didn't understand that command. Please try again.");
            }
            
            await next();
        });
    }

    async startNewThreadInChannel(context) {
        try {
            const teamsChannelId = context.activity.channelData.channel.id;
            const activity = MessageFactory.text("This will start a new thread in the channel.");
            
            const [reference] = await TeamsInfo.sendMessageToTeamsChannel(
                context,
                activity,
                teamsChannelId,
                process.env.MicrosoftAppId
            );

            await context.adapter.continueConversationAsync(
                process.env.MicrosoftAppId,
                reference,
                async (turnContext) => {
                    await turnContext.sendActivity("This will be the first response to the new thread.");
                }
            );
        } catch (error) {
            console.error('Error starting new thread:', error);
            await context.sendActivity("Error starting thread: " + error.message);
        }
    }

    async listTeamChannels(context) {
        try {
            const teamId = context.activity.channelData.team.id;
            const channels = await TeamsInfo.getTeamChannels(context, teamId);
            
            if (!channels.length) {
                await context.sendActivity("No channels found in this team.");
                return;
            }

            const cardContent = {
                type: "AdaptiveCard",
                version: "1.4",
                body: [{ type: "TextBlock", text: "List of Channels", weight: "Bolder", size: "Medium" }],
            };

            channels.forEach((channel, index) => {
                cardContent.body.push({
                    type: "TextBlock",
                    text: `${index + 1}. ${channel.name || 'General'}`,
                    wrap: true
                });
            });

            const cardAttachment = CardFactory.adaptiveCard(cardContent);
            await context.sendActivity({ attachments: [cardAttachment] });
        } catch (error) {
            console.error('Error listing channels:', error);
            await context.sendActivity("Error listing channels: " + error.message);
        }
    }

    async getTeamMember(context) {
        try {
            const aadObjectId = context.activity.from.aadObjectId;
            const teamId = context.activity.channelData.team.id;
            const teamMember = await TeamsInfo.getTeamMember(context, teamId, aadObjectId);
            
            if (!teamMember) {
                await context.sendActivity("Team member not found.");
                return;
            }

            const cardContent = {
                type: "AdaptiveCard",
                version: "1.4",
                body: [
                    { type: "TextBlock", text: "User Information", weight: "Bolder" },
                    { type: "FactSet", facts: [
                        { title: "Name:", value: teamMember.name },
                        { title: "Given Name:", value: teamMember.givenName || 'N/A' },
                        { title: "Surname:", value: teamMember.surname || 'N/A' },
                        { title: "Email:", value: teamMember.email || 'N/A' },
                        { title: "AAD Object ID:", value: teamMember.aadObjectId }
                    ]}
                ]
            };

            const cardAttachment = CardFactory.adaptiveCard(cardContent);
            await context.sendActivity({ attachments: [cardAttachment] });
        } catch (error) {
            console.error('Error getting team member:', error);
            await context.sendActivity("Error retrieving team member: " + error.message);
        }
    }

    async getPagedTeamMembers(context) {
        try {
            const teamId = context.activity.channelData.team.id;
            let members = [];
            let continuationToken = null;

            do {
                const currentPage = await TeamsInfo.getPagedTeamMembers(context, teamId, continuationToken);
                continuationToken = currentPage.continuationToken;
                members = members.concat(currentPage.members);
            } while (continuationToken);

            if (members.length === 0) {
                await context.sendActivity("No team members found.");
                return;
            }

            const cardContent = {
                type: "AdaptiveCard",
                version: "1.4",
                body: [{ type: "TextBlock", text: "🔹 **Team Members**", weight: "Bolder", size: "Medium" }]
            };

            members.forEach((member) => {
                cardContent.body.push({
                    type: "Container",
                    items: [
                        { type: "TextBlock", text: `**Name:** ${member.name}`, wrap: true },
                        { type: "TextBlock", text: `**Email:** ${member.email || 'N/A'}`, wrap: true },
                        { type: "TextBlock", text: `**Given Name:** ${member.givenName || 'N/A'}`, wrap: true },
                        { type: "TextBlock", text: `**Surname:** ${member.surname || 'N/A'}`, wrap: true },
                        { type: "TextBlock", text: `**Role:** ${member.role || 'N/A'}`, wrap: true },
                        { type: "TextBlock", text: `**User Principal Name:** ${member.userPrincipalName || 'N/A'}`, wrap: true },
                        { type: "TextBlock", text: "─────────────────────────", weight: "Lighter" }
                    ]
                });
            });

            const cardAttachment = CardFactory.adaptiveCard(cardContent);
            await context.sendActivity({ attachments: [cardAttachment] });
        } catch (error) {
            console.error('Error retrieving team members:', error);
            await context.sendActivity("Error retrieving team members: " + error.message);
        }
    }
}

module.exports.TeamsStartNewThreadInChannel = TeamsStartNewThreadInChannel;
