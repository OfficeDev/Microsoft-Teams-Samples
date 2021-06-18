const { ActivityHandler, TurnContext,CardFactory,TeamsInfo,MessageFactory } = require('botbuilder');
var Proactive_Messages =require('../Models/ProactiveAppIntallationHelper');

class ProactiveBot extends ActivityHandler {
    constructor(conversationReferences) {
        super();
        this.conversationReferences = conversationReferences;
        this.onConversationUpdate(async (context, next) => {
        });

        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let cnt = 0; cnt < membersAdded.length; cnt++) {
                if (membersAdded[cnt].id !== context.activity.recipient.id) {
                    this.addConversationReference(context.activity);
                }
            }
            await next();
        });

        this.onMessage(async (context, next) => {
            this.addConversationReference(context.activity);
            TurnContext.removeRecipientMention(context.activity);
            const text = context.activity.text.trim().toLocaleLowerCase();
            if (text.includes('install')) {
               await this.InstalledAppsinPersonalScopeAsync(context);
            } 
            else if (text.includes('send')) {
               await this.SendNotificationToAllUsersAsync(context);
            } 
        });
    }

    async InstalledAppsinPersonalScopeAsync(context) {
        let newInstallationCount=0;
        let existingAppCount=0;
        let result = ""; 
        const TeamMembers = await TeamsInfo.getPagedMembers(context);
         var Count = TeamMembers.members.map(async member => {
            const ref = TurnContext.getConversationReference(context.activity);
            ref.user = member;
             await context.adapter.createConversation(ref, async (context) => {
                const ref = TurnContext.getConversationReference(context.activity);
                    await context.adapter.continueConversation(ref, async (context) => {
                        result=await Proactive_Messages.InstallApp_PersonalScope(context.activity.conversation.tenantId,member.aadObjectId);
                });
            });
           return result;
        });
		(await Promise.all(Count)).forEach(function(Status_Code) {
            if(Status_Code==409) existingAppCount++;
            else if(Status_Code==201) newInstallationCount++;
		});
        await context.sendActivity(MessageFactory.text("Existing: "+existingAppCount+" \n\n Newly Installed: "+newInstallationCount));
    }

    async SendNotificationToAllUsersAsync(context) {
        const TeamMembers = await TeamsInfo.getPagedMembers(context);
        let msg_App_Cout=TeamMembers.members.length;
        TeamMembers.members.map(async member => {
            const ref = TurnContext.getConversationReference(context.activity);
            ref.user = member;
             await context.adapter.createConversation(ref, async (context) => {
                const ref = TurnContext.getConversationReference(context.activity);
                    await context.adapter.continueConversation(ref, async (context) => {
                         await context.sendActivity("Proactive hello.");
                });
            });
        });
        await context.sendActivity(MessageFactory.text("Message sent:"+msg_App_Cout));
    }

    addConversationReference(activity) {
        const conversationReference = TurnContext.getConversationReference(activity);
        this.conversationReferences[conversationReference.conversation.id] = conversationReference;
    }
}
module.exports.ProactiveBot = ProactiveBot;
