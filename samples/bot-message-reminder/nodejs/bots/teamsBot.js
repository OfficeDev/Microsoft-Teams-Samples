// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory, TurnContext } = require("botbuilder");
const { TaskModuleResponseFactory } = require("../models/taskModuleResponseFactory");
const schedule = require('node-schedule');
const taskDetails = {};
var conversationReferences = {};
var adapter;
var messageArray = [{
    Title: "Sample Message 1",
    Description: "Description for sample message 1"
}, {
    Title: "Sample Message 2",
    Description: "Description for sample message 2"
}, {
    Title: "Sample Message 3",
    Description: "Description for sample message 3"
}, {
    Title: "Sample Message 4",
    Description: "Description for sample message 4"
}];

class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;
        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let member = 0; member < membersAdded.length; member++) {
                if (membersAdded[member].id !== context.activity.recipient.id) {
                    await context.sendActivity("Hello and welcome! With this sample you can schedule a task and get reminder on the scheduled date and time.(use command 'create-reminder')");
                }
            }

            await next();
        });

        // this.onMessage(async (context, next) => {
        //     if (context.activity.text.toLowerCase().trim() == "create-reminder") {
        //         const userCard = CardFactory.adaptiveCard(this.adaptiveCardForTaskModule());
        //         await context.sendActivity({ attachments: [userCard] });
        //     }

        //     // By calling next() you ensure that the next BotHandler is run.
        //     await next();
        // });
    }

    async handleTeamsMessagingExtensionQuery(context, query) {
        const attachments = [];

        messageArray.forEach(obj => {
            // const heroCard = CardFactory.heroCard(obj.Title);
            const preview = CardFactory.thumbnailCard(obj.Title, obj.Description);
            preview.content.tap = { type: 'invoke', value: obj };
            const attachment = { ...preview, preview };
            attachments.push(attachment);
        });

        return {
            composeExtension: {
                type: 'result',
                attachmentLayout: 'list',
                attachments: attachments
            }
        };
    }

    async handleTeamsMessagingExtensionSelectItem(context, obj) {
        const adaptiveCard = CardFactory.adaptiveCard({
            $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
            body: [
                {
                    type: "TextBlock",
                    size: "Default",
                    weight: "Bolder",
                    text: "Please schedule the message"
                },
                {
                    type: "TextBlock",
                    size: "Default",
                    weight: "Default",
                    text: "Message title: " + obj.Title,
                    wrap: true
                },
                {
                    type: "TextBlock",
                    size: "Default",
                    weight: "Default",
                    text: "Message description: " + obj.Description,
                    wrap: true
                },
                {
                    type: "ActionSet",
                    actions: [
                        {
                            type: "Action.Submit",
                            title: "Schedule message",
                            data: {
                                msteams: {
                                    type: "task/fetch"
                                },
                                id: "schedule",
                                title: obj.Title,
                                description: obj.Description
                            }
                        }
                    ]
                }
            ],
            type: "AdaptiveCard",
            version: "1.2"
        });

        const attachment = { ...adaptiveCard, adaptiveCard };

        return {
            composeExtension: {
                type: 'result',
                attachmentLayout: 'list',
                attachments: [attachment]
            }
        };
    }

    // Handle task module fetch.
    handleTeamsTaskModuleFetch(context, taskModuleRequest) {
        const cardTaskFetchId = taskModuleRequest.data.id;
        var taskInfo = {}; // TaskModuleTaskInfo
        let title = taskModuleRequest.data.title;
        let description = taskModuleRequest.data.description;

        if (cardTaskFetchId == "schedule") {
            taskInfo.url = taskInfo.fallbackUrl = this.baseUrl + "/scheduleTask?title=" + title + "&description=" + description;
            taskInfo.height = 350;
            taskInfo.width = 350;
            taskInfo.title = "Schedule task";
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
        };

        this.saveTaskDetails(taskDetails);
        await context.sendActivity("Task submitted successfully. You will get reminder for the task at scheduled time");

        const currentUser = context.activity.from.id;
        conversationReferences[currentUser] = TurnContext.getConversationReference(context.activity);
        adapter = context.adapter;

        var year = taskModuleRequest.data.dateTime.substring(0, 4);
        var month = taskModuleRequest.data.dateTime.substring(5, 7);
        var day = taskModuleRequest.data.dateTime.substring(8, 10);
        var hour = taskModuleRequest.data.dateTime.substring(11, 13);
        var min = taskModuleRequest.data.dateTime.substring(14, 16);
        const date = new Date(year, month - 1, day, hour, min);

        const job = schedule.scheduleJob(date, async function () {
            await adapter.continueConversation(conversationReferences[currentUser], async turnContext => {
                const userCard = CardFactory.adaptiveCard({
                    $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
                    body: [
                        {
                            type: "TextBlock",
                            size: "Default",
                            weight: "Bolder",
                            text: "Reminder for scheduled message!"
                        },
                        {
                            type: "TextBlock",
                            size: "Default",
                            weight: "Default",
                            text: "Message title: " + taskDetails["taskDetails"].title,
                            wrap: true
                        },
                        {
                            type: "TextBlock",
                            size: "Default",
                            weight: "Default",
                            text: "Message description: " + taskDetails["taskDetails"].description,
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