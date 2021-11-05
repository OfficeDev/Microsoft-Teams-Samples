// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory } = require("botbuilder");
const { TaskModuleResponseFactory } = require("../models/taskModuleResponseFactory");
const taskDetails = {};
const schedule = require('node-schedule');

class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;
        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let member = 0; member < membersAdded.length; member++) {
                if (membersAdded[member].id !== context.activity.recipient.id) {
                    await context.sendActivity("Hello and welcome! With this sample you can schedule a task and get reminder on the scheduled date and time.(use command 'create-reminder').");
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

    handleTeamsTaskModuleFetch(context, taskModuleRequest) {
        const cardTaskFetchId = taskModuleRequest.data.id;
        var taskInfo = {}; // TaskModuleTaskInfo

        if (cardTaskFetchId == "schedule") {
            taskInfo.url = taskInfo.fallbackUrl = this.baseUrl + "/scheduleTask";
            taskInfo.height = 350;
            taskInfo.width = 350;
            taskInfo.title = "Schedule task";
        }
        return TaskModuleResponseFactory.toTaskModuleResponse(taskInfo);
    }

    async handleTeamsTaskModuleSubmit(context, taskModuleRequest) {
        // Create new object to save task details.
        let taskDetails = {
            title: taskModuleRequest.data.title,
            dateTime: taskModuleRequest.data.dateTime,
        };

        this.saveTaskDetails(taskDetails);
        await context.sendActivity("Task submitted successfully");
        const date = new Date(2021, 10, 5, 20, 6, 0);
        const job = schedule.scheduleJob(date, async function(){
           console.log("hey");
            //sendReminder("hello");
        // await context.sendActivity("Task submitted.");
          });

        return null;
    }

    // This method is used to save task details.
    saveTaskDetails(taskDetails) {
        taskDetails["taskDetails"] = taskDetails;
    }

    sendReminder(test){
        console.log(test);
    }

    adaptiveCardForTaskModule = () => ({
        $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
        body: [
            {
                type: "TextBlock",
                size: "Medium",
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