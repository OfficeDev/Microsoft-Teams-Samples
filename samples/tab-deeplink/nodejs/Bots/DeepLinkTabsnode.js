const {TeamsActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
var ACData = require("adaptivecards-templating");
var DeepLinkTabHelper=require("../pages/DeepLinkTabHelper.js");
var DeepLinkModel = require ('../pages/DeepLinkModel.js');
this.AppID=process.env.MicrosoftAppId;

class DeepLinkTabsnode extends TeamsActivityHandler {
  constructor() 
    {
        super();            
        this.onMessage(async (context, next) => {
        const replyText = `Echo: ${ context.activity.text }`;  
        let conversationType = context.activity.conversation.conversationType; 
        var EntityID="DeepLinkApp";    
        if(context.activity.conversation.conversationType==="channel")
        {
          var BotsDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel("topic1",1,"Bots",context.activity.channelData.teamsChannelId,process.env.MicrosoftAppId,EntityID);
          var MessagingDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel("topic2",2,"Messaging Extension",context.activity.channelData.teamsChannelId,process.env.MicrosoftAppId,EntityID);
          var AdaptiveCardDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel("topic3",3,"Adaptive Card",context.activity.channelData.teamsChannelId,process.env.MicrosoftAppId,EntityID);  
        }       
         else  if(context.activity.conversation.conversationType==="personal")
         {
          var BotsDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic("topic1",1,"Bots",process.env.MicrosoftAppId);
          var MessagingDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic("topic2",2,"Messaging Extension",process.env.MicrosoftAppId);
          var AdaptiveCardDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic("topic3",3,"Adaptive Card",process.env.MicrosoftAppId);            
         }
         
        await context.sendActivity({ attachments: [this.createAdaptiveCard(conversationType, BotsDeepLink,MessagingDeepLink,AdaptiveCardDeepLink)] }); 
        });

        this.onMembersAdded(async (context, next) => {
        const membersAdded = context.activity.membersAdded;
        const welcomeText = 'Hello and welcome!';
          for (let cnt = 0; cnt < membersAdded.length; ++cnt) {
            if (membersAdded[cnt].id !== context.activity.recipient.id) {
                  await context.sendActivity(MessageFactory.text(welcomeText, welcomeText));
            }
          }
          // By calling next() you ensure that the next BotHandler is run.
          await next();
        });
    };
    
    createAdaptiveCard(conversationType,BotsDeepLink,MessagingDeepLink,AdaptiveCardDeepLink){
      const fs = require('fs')
      const jsonContentStr = fs.readFileSync('resources/AdaptiveCard.json', 'utf8')
      var templatePayload = JSON.parse(jsonContentStr);
      var template = new ACData.Template(templatePayload);
  
        var cardPayload = template.expand({
          $root: {
            BotsDeepLink: BotsDeepLink.linkUrl,
            MEDeepLink: MessagingDeepLink.linkUrl,
            CardsDeepLink: AdaptiveCardDeepLink.linkUrl,
            BotsTitle: BotsDeepLink.TaskText,
            METitle: MessagingDeepLink.TaskText,
            CardsTitle: AdaptiveCardDeepLink.TaskText           
          }
        }); 
        return CardFactory.adaptiveCard(cardPayload);         
    }
}  

module.exports = DeepLinkTabsnode;