const { ActivityHandler, TurnContext, TeamsInfo, MessageFactory } = require('botbuilder');
const ProactiveAppInstallationHelper = require('../Models/ProactiveAppInstallationHelper');

class ProactiveBot extends ActivityHandler {
    constructor(conversationReferences) {
        super();
        this.conversationReferences = conversationReferences;

        this.onConversationUpdate(async (context, next) => {
            this.addConversationReference(context.activity);
            await next();
        });

        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (const member of membersAdded) {
                if (member.id !== context.activity.recipient.id) {
                    this.addConversationReference(context.activity);
                }
            }
            await next();
        });

        this.onMessage(async (context, next) => {
            TurnContext.removeRecipientMention(context.activity);
            const text = (context.activity.text || '').trim().toLocaleLowerCase();
            if (text.includes('install')) {
                await this.installAppInTeamsAndChatMembersPersonalScope(context);
            } else if (text.includes('send')) {
                await this.sendNotificationToAllUsersAsync(context);
            }
            await next();
        });
    }

    async installAppInTeamsAndChatMembersPersonalScope(context) {
        let newAppInstallCount = 0;
        let existingAppInstallCount = 0;
        const helper = new ProactiveAppInstallationHelper();
        const teamMembers = await TeamsInfo.getPagedMembers(context);

        const results = await Promise.all(
            teamMembers.members.map(async (member) => {
                if (!this.conversationReferences[member.aadObjectId]) {
                    return helper.installAppInPersonalScope(context.activity.conversation.tenantId, member.aadObjectId);
                }
                return null;
            })
        );

        for (const statusCode of results) {
            if (statusCode === 409) existingAppInstallCount++;
            else if (statusCode === 201) newAppInstallCount++;
        }

        await context.sendActivity(MessageFactory.text(`Existing: ${existingAppInstallCount} \n\n Newly Installed: ${newAppInstallCount}`));
    }

    async sendNotificationToAllUsersAsync(context) {
        const teamMembers = await TeamsInfo.getPagedMembers(context);
        const sentMsgCount = teamMembers.members.length;

        for (const member of teamMembers.members) {
            const conversationParameters = {
                isGroup: false,
                channelData: {
                    tenant: { id: context.activity.conversation.tenantId }
                },
                bot: context.activity.recipient,
                members: [member]
            };

            await context.adapter.createConversationAsync(
                process.env.MicrosoftAppId,
                context.activity.channelId,
                context.activity.serviceUrl,
                null,
                conversationParameters,
                async (turnContext) => {
                    await turnContext.sendActivity('Proactive hello.');
                }
            );
        }

        await context.sendActivity(MessageFactory.text(`Message sent: ${sentMsgCount}`));
    }

    addConversationReference(activity) {
        const conversationReference = TurnContext.getConversationReference(activity);
        this.conversationReferences[conversationReference.user.aadObjectId] = conversationReference;
    }
}

module.exports.ProactiveBot = ProactiveBot;