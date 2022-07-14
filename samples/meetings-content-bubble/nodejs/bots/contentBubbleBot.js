// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {TeamsActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
const{contentBubbleTitles}=require('../models/contentbubbleTitle');
const AdaptiveCard = require('../resources/adaptiveCard.json');
const templateJson = require('../resources/QuestionTemplate.json');
var ACData = require("adaptivecards-templating");

class ContentBubbleBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;
        this.AppId   = process.env.MicrosoftAppId;
        
        this.onMessage(async (context, next) => {
            if(context.activity.value==null){
                await context.sendActivity({ attachments: [this.createAdaptiveCard()] });
            }
            else{
                var json = JSON.stringify(context.activity.value);
                var out=JSON.parse(json);
                if(out.action=='inputselector'){
                  contentBubbleTitles.contentQuestion=out.myReview;
                   await this.contentBubble(context);
                    await context.sendActivity({ attachments: [this.createQuestionAdaptiveCard(out.myReview)] });
                }else {
                   await context.sendActivity(context.activity.from.name + " : " +"**"+out.myReview+"**"+" for " + "'" + out.action + "'");
                }
            }
         await next();
        });
    };

    async handleTeamsTaskModuleSubmit(context,taskModuleRequest) {
        var review = JSON.stringify(taskModuleRequest.data);
        var reply=JSON.parse(review);
        await context.sendActivity(context.activity.from.name + " : " +"**"+reply.myValue+"**"+" for " + "'" + reply.title + "'")
    }

    async contentBubble(context)
    {
      const replyActivity = MessageFactory.text("**Please provide your valuable feedback**");
      replyActivity.channelData = {
        onBehalfOf: [
          { 
            itemId: 0, 
            mentionType: 'person', 
            mri: context.activity.from.id, 
            displayname: context.activity.from.name 
          }
         ],
        notification: {
          alertInMeeting: true,
          externalResourceUrl: 'https://teams.microsoft.com/l/bubble/'+this.AppId+'?url='+this.baseUrl+'&height=270&width=300&title=ContentBubbleinTeams&completionBotId='+this.AppId
        }
      };
      await context.sendActivity(replyActivity);
    }

    createAdaptiveCard(){
      return CardFactory.adaptiveCard(AdaptiveCard);
    }

    createQuestionAdaptiveCard(myText){
    var templatePayload = templateJson;
        var template = new ACData.Template(templatePayload);
        var cardPayload = template.expand({
          $root: {
            name: myText
          }
        });
        return CardFactory.adaptiveCard(cardPayload);
  }
}
module.exports.ContentBubbleBot = ContentBubbleBot;
