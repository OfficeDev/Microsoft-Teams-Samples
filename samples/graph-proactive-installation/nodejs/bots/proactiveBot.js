const { ActivityHandler, TurnContext,CardFactory,TeamsInfo,MessageFactory } = require('botbuilder');
const ExistAdaptiveCard = require('../Resources/Exist_AdaptiveCard.json');
const NewAdaptiveCard = require('../Resources/New_AdaptiveCard.json');
const InstallAppCountCard = require('../Resources/InstallAppCountCard.json');
var ACData = require("adaptivecards-templating");
var Proactive_Messages =require('../Models/Proactivehelper');

class ProactiveBot extends ActivityHandler {
    constructor(conversationReferences) {
        super();
        let App_Status_Code;
        this.conversationReferences = conversationReferences;
        this.onConversationUpdate(async (context, next) => {
            switch(context.activity.conversation.conversationType)
            {
                case 'channel':
                    if(context.activity.membersAdded && typeof context.activity.membersAdded!=null)
                    {
                         App_Status_Code=await Proactive_Messages.CheckAppInstalledinChannelScope(context.activity.conversation.tenantId,context.activity.channelData.team.aadGroupId);
                    }
                    break;
                 case 'groupChat':
                    if(context.activity.membersAdded && typeof context.activity.membersAdded!=null)
                    {
                        App_Status_Code=await Proactive_Messages.CheckAppInstalledinGroupChatScope(context.activity.conversation.tenantId,context.activity.conversation.id);
                    }
                   break;
                 default:
                    break;
            }
            if(App_Status_Code==409)
                await context.sendActivity({attachments: [CardFactory.adaptiveCard(ExistAdaptiveCard)]});
            else if(App_Status_Code==201)
                await context.sendActivity({attachments: [CardFactory.adaptiveCard(NewAdaptiveCard)] });
            await this.InstallAppinGroupChatandChannel(context);       
        });

        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let cnt = 0; cnt < membersAdded.length; cnt++) {
                if (membersAdded[cnt].id !== context.activity.recipient.id) {
                    await context.sendActivity({ attachments:[CardFactory.adaptiveCard(NewAdaptiveCard)]});
                }
            }
            await next();
        });

        this.onMessage(async (context, next) => {
            this.addConversationReference(context.activity);
            TurnContext.removeRecipientMention(context.activity);
            const text = context.activity.text.trim().toLocaleLowerCase();
            if (text.includes('install')) {
                await this.CheckandInstallinPersonalScopeAsync(context);
            } 
            await next();
        });
    }

    addConversationReference(activity) {
        const conversationReference = TurnContext.getConversationReference(activity);
        this.conversationReferences[conversationReference.conversation.id] = conversationReference;
    }

    async InstallAppinGroupChatandChannel(context) {
        const TeamMembers = await TeamsInfo.getPagedMembers(context,10);
            TeamMembers.members.map(async member => {
            let Personal_StatusCode = "";            
            const ref = TurnContext.getConversationReference(context.activity);
            ref.user = member;
                await context.adapter.createConversation(ref, async (context) => {
                const ref = TurnContext.getConversationReference(context.activity);
                    await context.adapter.continueConversation(ref, async (context) => {
                    Personal_StatusCode=await Proactive_Messages.CheckAppInstalledinPersonalScope(context.activity.conversation.tenantId,member.aadObjectId);
                    if(Personal_StatusCode==201)
                        await context.sendActivity({ attachments:[CardFactory.adaptiveCard(NewAdaptiveCard)]});
                    else if(Personal_StatusCode==409)
                        await context.sendActivity({ attachments:[CardFactory.adaptiveCard(ExistAdaptiveCard)]});
                });
            });
        });
    }

    async CheckandInstallinPersonalScopeAsync(context) {
        let New_App_Cout=0;
        let Exist_App_Count=0;
        const TeamMembers = await TeamsInfo.getPagedMembers(context);
         var Count = TeamMembers.members.map(async member => {
            let Personal_StatusCode = "";            
            const ref = TurnContext.getConversationReference(context.activity);
            ref.user = member;
             await context.adapter.createConversation(ref, async (context) => {
                const ref = TurnContext.getConversationReference(context.activity);
                    await context.adapter.continueConversation(ref, async (context) => {
                    Personal_StatusCode=await Proactive_Messages.CheckAppInstalledinPersonalScope(context.activity.conversation.tenantId,member.aadObjectId);
                   if(Personal_StatusCode==201)
                        await context.sendActivity({ attachments:[CardFactory.adaptiveCard(NewAdaptiveCard)]});
                   else if(Personal_StatusCode==409)
                        await context.sendActivity({ attachments:[CardFactory.adaptiveCard(ExistAdaptiveCard)]});
                });
            });
           return Personal_StatusCode;
        });
    
		(await Promise.all(Count)).forEach(function(Status_Code) {
            if(Status_Code==409)
            {
                Exist_App_Count++;
            }
            else if(Status_Code==201)
            {
                New_App_Cout++;
            }
		});
        await context.sendActivity({ attachments:[this.IntstalledAppCountCard(New_App_Cout,Exist_App_Count)]});
    }

    IntstalledAppCountCard(New_App_Cout,Exist_App_Count){
        var template = new ACData.Template(InstallAppCountCard);
        var cardPayload = template.expand({
            $root: {
                New_App_Count:New_App_Cout,
                Exist_App_Count:Exist_App_Count
              }
         });
     return CardFactory.adaptiveCard(cardPayload);        
    }

}
module.exports.ProactiveBot = ProactiveBot;
