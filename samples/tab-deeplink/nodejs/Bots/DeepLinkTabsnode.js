const {TeamsActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
var ACData = require("adaptivecards-templating");
var SubTask=require("../public/SubTask.js");
var ChannelDeepLinkModel=require("../views/deeplinkmodelforChannel.js");

class DeepLinkTabsnode extends TeamsActivityHandler {
  constructor() 
    {
      debugger
        super();            
        this.onMessage(async (context, next) => {
        const replyText = `Echo: ${ context.activity.text }`;  
        if(context.activity.conversation.conversationType==="channel")
        {
          let task1DeepLink = ChannelDeepLinkModel.Task1DeepLink(context.activity.channelData.teamsChannelId)
          let task2DeepLink = ChannelDeepLinkModel.Task2DeepLink(context.activity.channelData.teamsChannelId)
          let task3DeepLink = ChannelDeepLinkModel.Task3DeepLink(context.activity.channelData.teamsChannelId)        
          await context.sendActivity({ attachments: [this.createAdaptiveCardForChannel(task1DeepLink,task2DeepLink,task3DeepLink)] });       
        }       
        else  if(context.activity.conversation.conversationType==="personal")
        await context.sendActivity({ attachments: [this.createAdaptiveCard()] });
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
  
  

 createAdaptiveCard(){
  var templatePayload = {
    "type": "AdaptiveCard",
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.0",
    "body": [
      {
        "type": "TextBlock",
        "text": "Please click on below buttons to navigate to a tab!",
        "wrap": true
      }      
    ],
    "actions": [
        {
        "type": "Action.OpenUrl",
        "title": "Bots",
        "url":"${Task1DeepLink}"
        },

        {
        "type": "Action.OpenUrl",
        "title": "Messaging Extension",
        "url":"${Task2DeepLink}"
        },

        {
       "type": "Action.OpenUrl",
        "title": "Adaptive Card",
        "url":"${Task3DeepLink}"
        }
      ]
    };
      var template = new ACData.Template(templatePayload);
      var cardPayload = template.expand({
        $root: {
          Task1DeepLink: SubTask.Task1DeepLink,
          Task2DeepLink: SubTask.Task2DeepLink,
          Task3DeepLink: SubTask.Task3DeepLink
        }
      });
      return CardFactory.adaptiveCard(cardPayload);        
    }

createAdaptiveCardForChannel(task1DeepLink,task2DeepLink,task3DeepLink){
      var templatePayload = {
        "type": "AdaptiveCard",
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.0",
        "body": [
          {
            "type": "TextBlock",
            "text": "Please click on below buttons to navigate to a tab!",
            "wrap": true
          }      
        ],
        "actions": [
            {
            "type": "Action.OpenUrl",
            "title": "Bots",
            "url":"${Task1DeepLink}"
            },
    
            {
            "type": "Action.OpenUrl",
            "title": "Messaging Extension",
            "url":"${Task2DeepLink}"
            },
    
            {
           "type": "Action.OpenUrl",
            "title": "Adaptive Card",
            "url":"${Task3DeepLink}"
            }
          ]
        };     
          var template = new ACData.Template(templatePayload);
          var cardPayload = template.expand({
            $root: {
              Task1DeepLink: task1DeepLink.linkUrl,
              Task2DeepLink: task2DeepLink.linkUrl,
              Task3DeepLink: task3DeepLink.linkUrl
              
            }
          }); 
          return CardFactory.adaptiveCard(cardPayload);        
    }
}  

module.exports = DeepLinkTabsnode;