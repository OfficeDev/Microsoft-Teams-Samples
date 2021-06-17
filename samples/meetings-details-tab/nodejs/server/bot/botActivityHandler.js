// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    TeamsActivityHandler
} = require('botbuilder');
const store = require('../services/store');
const { createAdaptiveCard } = require('../services/AdaptiveCardService');

class BotActivityHandler extends TeamsActivityHandler {
    constructor() {
        super();
        this.onConversationUpdate(async (context, next) => {
            store.setItem("conversationId", context.activity.conversation.id);
            store.setItem ("serviceUrl", context.activity.serviceUrl);
        });
        
        this.onMessage(async (context, next) => {
            const userName = context.activity.from.name;
            const data = context.activity.value;
            const answer = data.Feedback;
            
            
            const taskInfoList = store.getItem("agendaList");
            const taskInfo = taskInfoList.find(x=> x.Id === data.Choice);
            let personAnswered = taskInfo.personAnswered;
            if(!personAnswered){
                const obj = {};
                obj[answer] = [userName];
                personAnswered = obj;
            }else {
                if(personAnswered[answer]){
                    personAnswered[answer].push(userName);
                }
                else{
                    personAnswered[answer] = [userName];
                }
            }
            taskInfo.personAnswered = personAnswered;
            store.setItem("agendaList", taskInfoList);

            const option1Answered = personAnswered[taskInfo.option1] ? personAnswered[taskInfo.option1].length : 0;
            const option2Answered = personAnswered[taskInfo.option2] ? personAnswered[taskInfo.option2].length : 0;


            const total = option1Answered + option2Answered;
            const percentOption1 = total == 0 ? 0 : parseInt(( option1Answered * 100 ) / total);
            const percentOption2 = total == 0 ? 0 : 100 - percentOption1;
            
            const card = createAdaptiveCard("Result.json", taskInfo, percentOption1, percentOption2);
            await context.sendActivity({attachments: [card]});
        });
    }

    handleTeamsTaskModuleFetch(context, request) {
            const Id = request.data.Id;
            let taskInfo = {
                title: null,
                height: null,
                width: null,
                url: null,
                card: null,
                fallbackUrl: null,
                completionBotId: null,
            };
                taskInfo.url = process.env.BaseUrl +"/Result?id="+Id;
                taskInfo.title = "Result";
                taskInfo.height = 250;
                taskInfo.width = 500;
                taskInfo.fallbackUrl = taskInfo.url

                return {
                    task: {
                        type: 'continue',
                        value: taskInfo
                    }
                };
        }
}

module.exports.BotActivityHandler = BotActivityHandler;

