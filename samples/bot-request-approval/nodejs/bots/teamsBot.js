// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory, TeamsInfo } = require("botbuilder");
const adaptiveCards = require('../models/adaptiveCard');

const conversationDataReferences = {};

class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;
        
        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let member = 0; member < membersAdded.length; member++) {
                if (membersAdded[member].id !== context.activity.recipient.id) {
                    await context.sendActivity("Hello and welcome! With this sample you can send task request to your manager and your manager can approve/reject the request.");
                }
            }

            await next();
        });

        this.onMessage(async (context, next) => {
            await this.startIncManagement(context);

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }

    async onInvokeActivity(context) {
        console.log('Activity: ', context.activity.name);
        const user = context.activity.from;
        if (context.activity.name === 'adaptiveCard/action') {
            const action = context.activity.value.action;
            console.log('Verb: ', action.verb);
            const allMembers = await (await TeamsInfo.getMembers(context)).filter(tm => tm.aadObjectId);
            const card = await adaptiveCards.selectResponseCard(context, user, allMembers);
            return adaptiveCards.invokeResponse(card);
        }
    }

    async startIncManagement(context) {
        await context.sendActivity({
            attachments: [CardFactory.adaptiveCard(adaptiveCards.optionInc())]
        });
    }
}

module.exports.TeamsBot = TeamsBot;