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
                    await context.sendActivity("Hello and welcome! With this sample you can checkin your location (use command 'checkin') and view your checked in location(use command 'viewcheckin').");
                }
            }

            await next();
        });

        this.onMessage(async (context, next) => {
            // if (context.activity.text.toLowerCase().trim() == "createtask") {
            //     const userCard = CardFactory.adaptiveCard(this.adaptiveCardForTaskCreation());
            //     await context.sendActivity({ attachments: [userCard] });
            // }

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

    adaptiveCardForTaskCreation = () => ({
        $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
        body: [
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: "Enter Title: "
            },
            {
                type: "Input.Text",
                id: "title",
                placeholder: "Enter title"
            },
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: "Enter Description: "
            },
            {
                type: "Input.Text",
                id: "description",
                placeholder: "Enter description"
            },
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: "People Picker with Org search enabled"
            },
            {
                type: "Input.ChoiceSet",
                choices: [],
                "choices.data": {
                    type: "Data.Query",
                    dataset: "graph.microsoft.com/users"
                },
                id: "people-picker",
                value: "<AAD ID 1>"
            }
        ],
        actions: [
            {
                "type": "Action.Submit",
                "title": "Submit"
            }
        ],
        type: "AdaptiveCard",
        version: "1.2"
    });
}

module.exports.TeamsBot = TeamsBot;