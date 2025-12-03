// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory, TurnContext } = require("botbuilder");
const { TaskModuleResponseFactory } = require("../models/taskModuleResponseFactory");
const schedule = require('node-schedule');
const taskDetails = {};
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
                    await context.sendActivity("Hello and welcome! With this sample you can schedule a message reminder by selecting `...` over the message then select more action and then create-reminder and you wil get reminder of the message at scheduled date and time.')");
                }
            }

            await next();
        });
    }

    async handleTeamsMessagingExtensionFetchTask(context, action) {
        var title = "";
        var description = "";

        if (action.messagePayload.subject != null) {
            title = action.messagePayload.body.content;
            description = action.messagePayload.subject;
        }

        else {
            title = action.messagePayload.body.content;
        }

        return {
            task: {
                type: 'continue',
                value: {
                    width: 350,
                    height: 350,
                    title: 'Schedule task',
                    url: this.baseUrl + "/scheduleTask?title=" + title + "&description=" + description
                }
            }
        };
    }

    // Handle task module submit action.
    async  handleTeamsMessagingExtensionSubmitAction(context, action) {
        // Create new object to save task details.
        let taskDetails = {
            title: action.data.title,
            dateTime: action.data.dateTime,
            description: action.data.description,
        };

        this.saveTaskDetails(taskDetails);
        await context.sendActivity("Task submitted successfully. You will get reminder for the task at scheduled time");

        const currentUser = context.activity.from.id;
        conversationReferences[currentUser] = TurnContext.getConversationReference(context.activity);
        adapter = context.adapter;

        var dateLocal = new Date(action.data.dateTime);
        var dateLocalString = dateLocal.toLocaleString();
        var month = dateLocalString.substring(0, 2);
        var day = dateLocalString.substring(3, 5);
        var year = dateLocalString.substring(6, 10);
        var hour = dateLocal.getHours();
        var min = dateLocal.getMinutes();
        const scheduleDate = new Date(year, month -1, day, hour, min);

        const job = schedule.scheduleJob(scheduleDate, async function () {
            const botAppId = process.env.MicrosoftAppId || process.env.AAD_APP_CLIENT_ID || '';
            await adapter.continueConversationAsync(botAppId, conversationReferences[currentUser], async turnContext => {
                const userCard = CardFactory.adaptiveCard({
                    $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
                    body: [
                        {
                            type: "TextBlock",
                            size: "Default",
                            weight: "Bolder",
                            text: "Reminder for scheduled task!"
                        },
                        {
                            type: "TextBlock",
                            size: "Default",
                            weight: "Default",
                            text: "Task title: " + taskDetails["taskDetails"].title,
                            wrap: true
                        },
                        {
                            type: "TextBlock",
                            size: "Default",
                            weight: "Default",
                            text: "Task description: " + taskDetails["taskDetails"].description,
                            wrap: true
                        },
                    ],
                    type: "AdaptiveCard",
                    version: "1.2"
                });

                await turnContext.sendActivity({ attachments: [userCard] });
            });
        });

        return null;
    }

    // This method is used to save task details.
    saveTaskDetails(taskDetails) {
        taskDetails["taskDetails"] = taskDetails;
    }

    // This method is used to create adaptive card.
    adaptiveCardForTaskModule = () => ({
        $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
        body: [
            {
                type: "TextBlock",
                size: "Default",
                weight: "Bolder",
                text: "Please click on schedule to schedule task"
            },
            {
                type: "ActionSet",
                actions: [
                    {
                        type: "Action.Submit",
                        title: "Schedule task",
                        data: {
                            msteams: {
                                type: "task/fetch"
                            },
                            id: "schedule"
                        }
                    }
                ]
            }
        ],
        type: "AdaptiveCard",
        version: "1.2"
    });
}

module.exports.TeamsBot = TeamsBot;