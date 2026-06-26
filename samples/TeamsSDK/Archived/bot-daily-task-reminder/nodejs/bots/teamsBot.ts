// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import {
    TeamsActivityHandler,
    CardFactory,
    TurnContext,
    ConversationReference,
    TaskModuleRequest,
    TaskModuleResponse,
} from 'botbuilder';
import schedule from 'node-schedule';

const conversationReferences: Record<string, Partial<ConversationReference>> = {};
let adapterRef: InstanceType<typeof import('botbuilder').CloudAdapter>;

export class TeamsBot extends TeamsActivityHandler {
    private baseUrl: string;

    constructor() {
        super();
        this.baseUrl = process.env.BaseUrl || '';

        this.onMembersAdded(async (context, next) => {
            for (const member of context.activity.membersAdded || []) {
                if (member.id !== context.activity.recipient.id) {
                    await context.sendActivity(
                        "Hello and welcome! With this sample, you can schedule a recurring task and receive reminders at the scheduled time. Use the command 'create-reminder' to start."
                    );
                }
            }
            await next();
        });

        this.onMessage(async (context, next) => {
            if (context.activity.text?.toLowerCase().trim() === 'create-reminder') {
                const userCard = CardFactory.adaptiveCard(this.adaptiveCardForTaskModule());
                await context.sendActivity({ attachments: [userCard] });
            }
            await next();
        });
    }

    async handleTeamsTaskModuleFetch(
        _context: TurnContext,
        taskModuleRequest: TaskModuleRequest
    ): Promise<TaskModuleResponse> {
        if (taskModuleRequest.data?.id === 'schedule') {
            return {
                task: {
                    type: 'continue',
                    value: {
                        url: `${this.baseUrl}/scheduleTask`,
                        fallbackUrl: `${this.baseUrl}/scheduleTask`,
                        height: 350,
                        width: 350,
                        title: 'Schedule a task',
                    },
                },
            };
        }
        return {};
    }

    async handleTeamsTaskModuleSubmit(
        context: TurnContext,
        taskModuleRequest: TaskModuleRequest
    ): Promise<TaskModuleResponse> {
        const { title, dateTime, description, selectedDays } = taskModuleRequest.data;

        await context.sendActivity(
            'Task submitted successfully, you will get a recurring reminder for the task at a scheduled time'
        );

        const currentUser = context.activity.from.id;
        conversationReferences[currentUser] = TurnContext.getConversationReference(context.activity);
        adapterRef = context.adapter as typeof adapterRef;

        const date = new Date(dateTime);
        const cronExpression = `${date.getMinutes()} ${date.getHours()} * * ${selectedDays.toString()}`;

        schedule.scheduleJob(cronExpression, async () => {
            try {
                const botAppId = process.env.MicrosoftAppId || process.env.AAD_APP_CLIENT_ID || '';
                await adapterRef.continueConversationAsync(
                    botAppId,
                    conversationReferences[currentUser],
                    async (turnContext: TurnContext) => {
                        const reminderCard = CardFactory.adaptiveCard({
                            $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
                            type: 'AdaptiveCard',
                            version: '1.2',
                            body: [
                                {
                                    type: 'TextBlock',
                                    size: 'Default',
                                    weight: 'Bolder',
                                    text: 'Reminder for a scheduled task!',
                                },
                                {
                                    type: 'TextBlock',
                                    size: 'Default',
                                    weight: 'Default',
                                    text: `Task title: ${title}`,
                                    wrap: true,
                                },
                                {
                                    type: 'TextBlock',
                                    size: 'Default',
                                    weight: 'Default',
                                    text: `Task description: ${description}`,
                                    wrap: true,
                                },
                            ],
                        });
                        await turnContext.sendActivity({ attachments: [reminderCard] });
                    }
                );
            } catch (err) {
                console.error('Error sending proactive reminder:', err);
            }
        });

        return {};
    }

    private adaptiveCardForTaskModule() {
        return {
            $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
            type: 'AdaptiveCard',
            version: '1.2',
            body: [
                {
                    type: 'TextBlock',
                    size: 'Default',
                    weight: 'Bolder',
                    text: 'Please click on schedule to schedule a task',
                },
                {
                    type: 'ActionSet',
                    actions: [
                        {
                            type: 'Action.Submit',
                            title: 'Schedule task',
                            data: {
                                msteams: { type: 'task/fetch' },
                                id: 'schedule',
                            },
                        },
                    ],
                },
            ],
        };
    }
}
