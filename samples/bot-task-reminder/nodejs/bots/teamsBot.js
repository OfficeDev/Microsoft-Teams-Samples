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
                    await context.sendActivity("Hello and welcome! With this sample you can schedule a recurring task and get a reminder on the scheduled time.(use command 'create-reminder')");
                }
            }

            await next();
        });

        this.onMessage(async (context, next) => {
            if (context.activity.text.toLowerCase().trim() == "create-reminder") {
                const userCard = CardFactory.adaptiveCard(this.adaptiveCardForTaskModule());
                await context.sendActivity({ attachments: [userCard] });
            }

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }

    // Handle task module fetch.
    handleTeamsTaskModuleFetch(context, taskModuleRequest) {
        const cardTaskFetchId = taskModuleRequest.data.id;
        var taskInfo = {}; // TaskModuleTaskInfo

        if (cardTaskFetchId == "schedule") {
            taskInfo.url = taskInfo.fallbackUrl = this.baseUrl + "/scheduleTask";
            taskInfo.height = 350;
            taskInfo.width = 350;
            taskInfo.title = "Schedule a task";
        }

        return TaskModuleResponseFactory.toTaskModuleResponse(taskInfo);
    }

    // Handle task module submit action.
    async handleTeamsTaskModuleSubmit(context, taskModuleRequest) {
        // Create new object to save task details.
        let taskDetails = {
            title: taskModuleRequest.data.title,
            dateTime: taskModuleRequest.data.dateTime,
            description: taskModuleRequest.data.description,
            selectedDays: taskModuleRequest.data.selectedDays,
        };

        this.saveTaskDetails(taskDetails);
        await context.sendActivity("Task submitted successfully, you will get a recurring reminder for the task at a scheduled time");

        const currentUser = context.activity.from.id;
        conversationReferences[currentUser] = TurnContext.getConversationReference(context.activity);
        adapter = context.adapter;

        var year = taskModuleRequest.data.dateTime.substring(0, 4);
        var month = taskModuleRequest.data.dateTime.substring(5, 7);
        var day = taskModuleRequest.data.dateTime.substring(8, 10);
        var hour = taskModuleRequest.data.dateTime.substring(11, 13);
        var min = taskModuleRequest.data.dateTime.substring(14, 16);
        var days = taskModuleRequest.data.selectedDays.toString();
        const date = new Date(year, month - 1, day, hour, min);
        var cronExpression = min+' '+hour+' * * '+days;

        const job = schedule.scheduleJob(cronExpression, async function () {
            await adapter.continueConversation(conversationReferences[currentUser], async turnContext => {
                const userCard = CardFactory.adaptiveCard({
                    $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
                    body: [
                        {
                            type: "TextBlock",
                            size: "Default",
                            weight: "Bolder",
                            text: "Reminder for a scheduled task!"
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