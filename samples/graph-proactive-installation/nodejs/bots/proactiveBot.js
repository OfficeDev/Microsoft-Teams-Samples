const { stripMentionsText } = require("@microsoft/teams.api");
const ProactiveAppIntallationHelper = require('../Models/ProactiveAppIntallationHelper');

class ProactiveBot {
    constructor(app) {
        this.app = app;
        this.conversationReferences = {};
        
        // Setup message handler
        this.setupHandlers();
    }

    setupHandlers() {
        // Handle incoming messages
        this.app.on('message', async (context) => {
            const { activity, send, api } = context;
            
            // Store conversation reference
            this.addConversationReference(activity);
            
            // Get message text without mentions
            const text = stripMentionsText(activity).trim().toLowerCase();
            
            if (text.includes('install')) {
                await this.InstallAppInTeamsAndChatMembersPersonalScope(context);
            } else if (text.includes('send')) {
                await this.SendNotificationToAllUsersAsync(context);
            } else {
                await send(`You said: ${stripMentionsText(activity)}`);
            }
        });

        // Handle members added
        this.app.on('members.added', async (context) => {
            const { activity } = context;
            this.addConversationReference(activity);
        });

        // Handle conversation update
        this.app.on('conversation.update', async (context) => {
            const { activity } = context;
            this.addConversationReference(activity);
        });
    }

    async InstallAppInTeamsAndChatMembersPersonalScope(context) {
        let NewAppInstallCount = 0;
        let ExistingAppInstallCount = 0;
        let result = "";
        const { activity, send, api } = context;
        
        try {
            const objProactiveAppIntallationHelper = new ProactiveAppIntallationHelper();
            
            // Get team members using Teams SDK API
            const members = await api.conversations.members(activity.conversation.id).get();
            
            // Get tenant ID from activity
            const tenantId = activity.conversation.tenantId;
            
            let Count = members.map(async member => {
                if (!this.conversationReferences[member.aadObjectId]) {
                    result = await objProactiveAppIntallationHelper.InstallAppInPersonalScope(tenantId, member.aadObjectId);
                }
                return result;
            });
            
            (await Promise.all(Count)).forEach(function (Status_Code) {
                if (Status_Code == 409) ExistingAppInstallCount++;
                else if (Status_Code == 201) NewAppInstallCount++;
            });
            
            await send("Existing: " + ExistingAppInstallCount + " \n\n Newly Installed: " + NewAppInstallCount);
        } catch (error) {
            console.error('Error installing app:', error);
            await send("Error installing app. Please check the logs.");
        }
    }

    async SendNotificationToAllUsersAsync(context) {
        const { activity, send, api } = context;
        
        try {
            // Get team members
            const members = await api.conversations.members(activity.conversation.id).get();
            let successCount = 0;
            
            // Send proactive messages to all members
            for (const member of members) {
                try {
                    // Create 1:1 conversation with the user
                    const conversationParams = {
                        bot: {
                            id: activity.recipient.id,
                            name: activity.recipient.name
                        },
                        members: [member],
                        channelData: {
                            tenant: {
                                id: activity.conversation.tenantId
                            }
                        },
                        isGroup: false,
                        conversationType: 'personal',
                        tenantId: activity.conversation.tenantId
                    };
                    
                    const conversationResourceResponse = await api.conversations.create(conversationParams);
                    
                    // Send message using the app.send method
                    await this.app.send(conversationResourceResponse.id, {
                        type: 'message',
                        text: 'Proactive hello.'
                    });
                    
                    successCount++;
                } catch (error) {
                    console.error(`Error sending message to user ${member.id}:`, error);
                }
            }
            
            await send(`Message sent to ${successCount} out of ${members.length} users.`);
        } catch (error) {
            console.error('Error sending notifications:', error);
            await send("Error sending notifications. Please check the logs.");
        }
    }

    addConversationReference(activity) {
        if (activity.from && activity.from.aadObjectId) {
            const conversationReference = {
                activityId: activity.id,
                user: activity.from,
                bot: activity.recipient,
                conversation: activity.conversation,
                channelId: activity.channelId,
                serviceUrl: activity.serviceUrl
            };
            this.conversationReferences[activity.from.aadObjectId] = conversationReference;
        }
    }
}

module.exports.ProactiveBot = ProactiveBot;
