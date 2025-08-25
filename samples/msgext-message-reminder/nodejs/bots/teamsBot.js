// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
 
const { TeamsActivityHandler, CardFactory, TurnContext } = require("botbuilder");
const schedule = require('node-schedule');
 
var conversationReferences = {};
var adapter;
 
class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;
 
        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let member = 0; member < membersAdded.length; member++) {
                if (membersAdded[member].id !== context.activity.recipient.id) {
                    await context.sendActivity("Hello and welcome! With this sample you can schedule a message reminder by selecting `...` over the message then select more action and then create-reminder and you will get a reminder of the message at scheduled date and time.");
                }
            }
            await next();
        });
    }
 
    async handleTeamsMessagingExtensionFetchTask(context, action) {
        let title = "";
        let description = "";
 
        if (action.messagePayload.subject != null) {
            title = action.messagePayload.body.content;
            description = action.messagePayload.subject;
        } else {
            title = action.messagePayload.body.content;
        }
 
        return {
            task: {
                type: 'continue',
                value: {
                    width: 350,
                    height: 350,
                    title: 'Schedule task',
                    url: `${this.baseUrl}/scheduleTask?title=${encodeURIComponent(title)}&description=${encodeURIComponent(description)}`
                }
            }
        };
    }
 
    async handleTeamsMessagingExtensionSubmitAction(context, action) {
        const currentUser = context.activity.from.id;
        const userConversationReference = TurnContext.getConversationReference(context.activity);
        conversationReferences[currentUser] = userConversationReference;
        adapter = context.adapter;
 
        const { title, description, dateTime } = action.data;
 
        const taskDetails = {
            title,
            description,
            dateTime
        };
 
        await context.sendActivity("Task submitted successfully. You will get a reminder at the scheduled time.");
 
        const scheduleDate = new Date(dateTime); // already a UTC-compliant ISO string from task module
 
        schedule.scheduleJob(scheduleDate, async () => {
            await adapter.continueConversation(conversationReferences[currentUser], async turnContext => {
                const reminderCard = CardFactory.adaptiveCard({
                    $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
                    type: "AdaptiveCard",
                    version: "1.2",
                    body: [
                        {
                            type: "TextBlock",
                            text: "Reminder for scheduled task!",
                            weight: "Bolder",
                            size: "Medium"
                        },
                        {
                            type: "TextBlock",
                            text: `Task title: ${taskDetails.title}`,
                            wrap: true
                        },
                        {
                            type: "TextBlock",
                            text: `Task description: ${taskDetails.description}`,
                            wrap: true
                        }
                    ]
                });
 
                await turnContext.sendActivity({ attachments: [reminderCard] });
            });
        });
 
        return null;
    }
}
 
module.exports.TeamsBot = TeamsBot;