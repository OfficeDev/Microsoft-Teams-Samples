const {TeamsActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
var ACData = require("adaptivecards-templating");
var SubTask=require("../public/SubTask.js");
var AdaptiveCard=require("../resources/adaptivecard.json")

class DeepLinkTabsnode extends TeamsActivityHandler {
  constructor() 
    {
        super();      
        this.onMessage(async (context, next) => {
        const replyText = `Echo: ${ context.activity.text }`;     
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
    async handleTeamsTaskModuleSubmit(context,taskModuleRequest) {
      var review = JSON.stringify(taskModuleRequest.data);
      var reply=JSON.parse(review);
      await context.sendActivity(context.activity.from.name + " : " +"**"+reply.myValue+"**"+" for " + "'" + reply.title + "'")
    }
    async DeepLink(context)
    {      
    const replyActivity = MessageFactory.text("**Please provide your valuable feedback**");
    replyActivity.channelData = {
      notification: {
        alertInMeeting: true,
        externalResourceUrl: 'https://teams.microsoft.com/l/bubble/<<APP_ID>>?url=<<ENDPOINT_URL>>&height=270&width=300&title=ContentBubbleinTeams&completionBotId=<<APP_ID>>'
        
      }
    };
    await context.sendActivity(replyActivity);
  }

 createAdaptiveCard(){
  var templatePayload = {
    "type": "AdaptiveCard",
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.0",
    "body": [
      {
        "type": "TextBlock",
        "text": "Hey Olo Brockhouse! Please click on below buttons to navigate to a tab!",
        "wrap": true
      }      
    ],
    "actions": [
        {
        "type": "Action.OpenUrl",
        "title": "topic1",
        "url":"${Task1DeepLink}"
        },

        {
        "type": "Action.OpenUrl",
        "title": "topic2",
        "url":"${Task2DeepLink}"
        },

        {
       "type": "Action.OpenUrl",
        "title": "topic3",
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
}    
module.exports = DeepLinkTabsnode;