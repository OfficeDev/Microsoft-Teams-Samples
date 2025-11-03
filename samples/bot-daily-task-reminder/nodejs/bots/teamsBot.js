// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory, TurnContext } = require("botbuilder");
const { TaskModuleResponseFactory } = require("../models/taskModuleResponseFactory");
const schedule = require('node-schedule');
const conversationReferences = {};
let adapter;

/**
 * TeamsBot class handles Teams activities and task modules.
 */
class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;

        // Handle when a new member is added to the conversation.
        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (const member of membersAdded) {
                if (member.id !== context.activity.recipient.id) {
                    await context.sendActivity("Hello and welcome! With this sample, you can schedule a recurring task and receive reminders at the scheduled time. Use the command 'create-reminder' to start.");
                }
            }
            await next();
        });

        // Handle incoming messages.
        this.onMessage(async (context, next) => {
            if (context.activity.text.toLowerCase().trim() === "create-reminder") {
                const userCard = CardFactory.adaptiveCard(this.adaptiveCardForTaskModule());
                await context.sendActivity({ attachments: [userCard] });
            }
            await next();
        });
    }

    /**
     * Handle task module fetch.
     * @param {TurnContext} context - The context object for the turn.
     * @param {Object} taskModuleRequest - The task module request object.
     * @returns {Object} - The task module response.
     */
    handleTeamsTaskModuleFetch(context, taskModuleRequest) {
        const cardTaskFetchId = taskModuleRequest.data.id;

        if (cardTaskFetchId === "schedule") {
            return TaskModuleResponseFactory.toTaskModuleResponse({
                url: `${this.baseUrl}/scheduleTask`,
                fallbackUrl: `${this.baseUrl}/scheduleTask`,
                height: 350,
                width: 350,
                title: "Schedule a task"
            });
        }
        return null;
    }

    /**
     * Handle task module submit action.
     * @param {TurnContext} context - The context object for the turn.
     * @param {Object} taskModuleRequest - The task module request object.
     */
    async handleTeamsTaskModuleSubmit(context, taskModuleRequest) {
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

        const date = new Date(taskModuleRequest.data.dateTime);
        const cronExpression = `${date.getMinutes()} ${date.getHours()} * * ${taskModuleRequest.data.selectedDays.toString()}`;

        schedule.scheduleJob(cronExpression, async () => {
            try {
                const botAppId = process.env.MicrosoftAppId || process.env.AAD_APP_CLIENT_ID || '';
                if (!botAppId) {
                    console.warn('MicrosoftAppId is not set in environment. Proactive send may fail.');
                }
                await adapter.continueConversationAsync(botAppId, conversationReferences[currentUser], async turnContext => {
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
                            }
                        ],
                    type: "AdaptiveCard",
                    version: "1.2"
                    });

                    await turnContext.sendActivity({ attachments: [userCard] });
                });
            }
            catch (err) {
                console.error('Error sending proactive reminder:', err && err.stack ? err.stack : err);
            }
        });

        return null;
    }

    /**
     * Save task details.
     * @param {Object} taskDetails - The task details object.
     */
    saveTaskDetails(taskDetails) {
        this.taskDetails = taskDetails;
    }

    /**
     * Create the adaptive card for the task module.
     * @returns {Object} - The adaptive card object.
     */
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