// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory, TurnContext } = require("botbuilder");
const { TaskModuleResponseFactory } = require("../models/taskModuleResponseFactory");
const schedule = require('node-schedule');
const conversationReferences = {};
let adapter;

class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;

        // Handle when a new member is added to the conversation.
        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let member of membersAdded) {
                if (member.id !== context.activity.recipient.id) {
                    await context.sendActivity("Hello and welcome! With this sample, you can schedule a recurring task and receive reminders at the scheduled time. Use the command 'create-reminder' to start.");
                }
            }

            await next();
        });

        // Handle incoming messages.
        this.onMessage(async (context, next) => {
            if (context.activity.text.toLowerCase().trim() == "create-reminder") {
                const userCard = CardFactory.adaptiveCard(this.adaptiveCardForTaskModule());
                await context.sendActivity({ attachments: [userCard] });
            }

            await next();
        });
    }

    // Handle task module fetch.
    handleTeamsTaskModuleFetch(context, taskModuleRequest) {
        const cardTaskFetchId = taskModuleRequest.data.id;

        if (cardTaskFetchId == "schedule") {
            return TaskModuleResponseFactory.toTaskModuleResponse({
                url: this.baseUrl + "/scheduleTask",
                fallbackUrl: this.baseUrl + "/scheduleTask",
                height: 350,
                width: 350,
                title: "Schedule a task"
            });
        }
        return null;
    }

    // Handle task module submit action.
    async handleTeamsTaskModuleSubmit(context, taskModuleRequest) {
        // Save task details locally.
        const taskDetails = {
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

        // Parse the task datetime and create a cron expression for scheduling.
        const date = new Date(taskModuleRequest.data.dateTime);
        const year = date.getFullYear();
        const month = date.getMonth() + 1; // Months are 0-indexed
        const day = date.getDate();
        const hour = date.getHours();
        const min = date.getMinutes();
        const days = taskModuleRequest.data.selectedDays.toString();

        const cronExpression = `${min} ${hour} * * ${days}`;

        // Schedule the recurring task reminder using node-schedule.
        schedule.scheduleJob(cronExpression, async function () {
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
                            text: `Task title: ${taskDetails.title}`,
                            wrap: true
                        },
                        {
                            type: "TextBlock",
                            size: "Default",
                            weight: "Default",
                            text: `Task description: ${taskDetails.description}`,
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
        // Store task details (no need for additional nesting here)
        this.taskDetails = taskDetails;
    }

    // This method is used to create the adaptive card for the task module.
    adaptiveCardForTaskModule = () => ({
        $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
        body: [
            {
                type: "TextBlock",
                size: "Default",
                weight: "Bolder",
                text: "Please click on schedule to schedule a task"
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
