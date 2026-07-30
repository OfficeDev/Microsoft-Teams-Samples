// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory, TurnContext } = require("botbuilder");
const schedule = require('node-schedule');

const conversationReferences = {};
let adapter;

class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl;

        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;

            for (const member of membersAdded) {
                if (member.id !== context.activity.recipient.id) {
                    await context.sendActivity("Hello and welcome! With this sample you can schedule a message reminder by selecting `...` over the message then select more actions and then create-reminder. You will get a reminder of the message at the scheduled date and time.");
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

        // Strip HTML tags and trim to keep the URL within length limits.
        const cleanTitle = title.replace(/<[^>]*>/g, '').trim().substring(0, 200);
        const cleanDescription = description.replace(/<[^>]*>/g, '').trim().substring(0, 200);

        const url = `${this.baseUrl}/scheduleTask?title=${encodeURIComponent(cleanTitle)}&description=${encodeURIComponent(cleanDescription)}`;

        return {
            task: {
                type: 'continue',
                value: {
                    width: 350,
                    height: 350,
                    title: 'Schedule task',
                    url: url
                }
            }
        };
    }

    async handleTeamsMessagingExtensionSubmitAction(context, action) {
        const taskInfo = {
            title: action.data.title,
            dateTime: action.data.dateTime,
            description: action.data.description,
        };

        await context.sendActivity("Task submitted successfully. You will get a reminder for the task at the scheduled time.");

        const currentUser = context.activity.from.id;
        conversationReferences[currentUser] = TurnContext.getConversationReference(context.activity);
        adapter = context.adapter;

        const scheduleDate = new Date(action.data.dateTime);

        schedule.scheduleJob(scheduleDate, async function () {
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
                            text: "Task title: " + taskInfo.title,
                            wrap: true
                        },
                        {
                            type: "TextBlock",
                            size: "Default",
                            weight: "Default",
                            text: "Task description: " + taskInfo.description,
                            wrap: true
                        },
                    ],
                    type: "AdaptiveCard",
                    version: "1.4"
                });

                await turnContext.sendActivity({ attachments: [userCard] });
            });
        });

        return null;
    }
}

module.exports.TeamsBot = TeamsBot;