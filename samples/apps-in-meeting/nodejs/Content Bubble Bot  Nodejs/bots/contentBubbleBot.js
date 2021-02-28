// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {ActivityHandler,TeamsActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
const { TaskModuleUIConstants } = require('../models/TaskModuleUIConstants');
const { TaskModuleIds } = require('../models/taskmoduleids');
const { TaskModuleResponseFactory } = require('../models/taskmoduleresponsefactory');
const AdaptiveCard = require('../resources/adaptiveCard.json');
const FirstTitile = require('../resources/adaptiveFirstTitle.json');
const SecondTitle = require('../resources/adaptiveSecondTitle.json');
const ThirdTitle = require('../resources/adaptiveThirdTitle.json');
const { json } = require('express');

class ContentBubbleBot extends TeamsActivityHandler {
    constructor() {
        super();

        this.baseUrl = process.env.BaseUrl;

         this.constMethod= {
            firstTitle : 'Approve 5% dividend payment to shareholders',
            secondTitle : 'Increase research budget by 10%',
            thirdTitle : "Continue with WFH for next 3 months"
        }

        this.onMessage(async (context, next) => {
            if(context.activity.value==null){
                await context.sendActivity({ attachments: [this.createAdaptiveCard()] });
            }
            else{
                var json = JSON.stringify(context.activity.value);
                var out = JSON.parse(json);

                if(out.myReview== this.constMethod.firstTitle){
                    await this.contentBubbleFirst(context);
                    await context.sendActivity({ attachments: [this.createFirstTitleAdaptiveCard()] });
                }else if(out.myReview== this.constMethod.secondTitle){
                    await this.contentBubbleSecond(context);
                    await context.sendActivity({ attachments: [this.createSecondTitleAdaptiveCard()] });
                }else if(out.myReview==this.constMethod.thirdTitle){
                    await this.contentBubbleThird(context);
                    await context.sendActivity({ attachments: [this.createThirdTitleAdaptiveCard()] });
                }
                else if(out.myReview== "yes"){
                    await context.sendActivity(context.activity.from.name + " : " + "**Yes** for " + "'" + out.action + "'");
                }else if(out.myReview== "no"){
                    await context.sendActivity(context.activity.from.name + " : " + "**No** for " + "'" + out.action + "'");
                }
            }

         await next();
        });
    };

    async handleTeamsTaskModuleSubmit(context,taskModuleRequest) {
        var review = JSON.stringify(taskModuleRequest.data);
        var reply=JSON.parse(review);
        if(reply.myValue=="yes"){
            await context.sendActivity(context.activity.from.name + " : " + "**Yes** for " + "'"+ reply.title + "'")
        }else{
            await context.sendActivity(context.activity.from.name + " : " + "**No** for " + "'"+ reply.title + "'")
        }
    }

    createAdaptiveCard(){
        return CardFactory.adaptiveCard(AdaptiveCard);
    }
    createFirstTitleAdaptiveCard(){
        return CardFactory.adaptiveCard(FirstTitile);
    }
    createSecondTitleAdaptiveCard(){
        return CardFactory.adaptiveCard(SecondTitle);
    }
    createThirdTitleAdaptiveCard(){
        return CardFactory.adaptiveCard(ThirdTitle);
    }

  
   async contentBubbleFirst(context)
    {
        const replyActivity = MessageFactory.text("**Please provide your valuable feedback**"); // this could be an adaptive card instead
        replyActivity.channelData = {
            notification: {
            alertInMeeting: true,
            externalResourceUrl: 'https://teams.microsoft.com/l/bubble/MICROSOFT-APP-ID?url=https://YOUR_ENDPOINT_URL&height=270&width=300&title=ContentBubbleinTeams&completionBotId=MICROSOFT-APP-ID'
        }
       };
     await context.sendActivity(replyActivity);
    }

    async contentBubbleSecond(context)
    {
        const replyActivity = MessageFactory.text("**Please provide your valuable feedback**"); // this could be an adaptive card instead
        replyActivity.channelData = {
            notification: {
            alertInMeeting: true,
            externalResourceUrl: 'https://teams.microsoft.com/l/bubble/MICROSOFT-APP-ID?url=https://YOUR_ENDPOINT_URL&height=270&width=300&title=ContentBubbleinTeams&completionBotId=MICROSOFT-APP-ID'
        }
       };
     await context.sendActivity(replyActivity);
    }

    async contentBubbleThird(context)
    {
        const replyActivity = MessageFactory.text("**Please provide your valuable feedback**"); // this could be an adaptive card instead
        replyActivity.channelData = {
            notification: {
            alertInMeeting: true,
            externalResourceUrl: 'https://teams.microsoft.com/l/bubble/MICROSOFT-APP-ID?url=https://YOUR_ENDPOINT_URL&height=270&width=300&title=ContentBubbleinTeams&completionBotId=MICROSOFT-APP-ID'
        }
       };
     await context.sendActivity(replyActivity);
    }
}

module.exports.ContentBubbleBot = ContentBubbleBot;
