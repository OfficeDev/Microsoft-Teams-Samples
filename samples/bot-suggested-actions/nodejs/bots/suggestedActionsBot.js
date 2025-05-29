// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityHandler, MessageFactory } = require('botbuilder');
const { ActionTypes } = require('botframework-schema');

/**
 * SuggestedActionsBot handles incoming messages and sends suggested actions.
 */
class SuggestedActionsBot extends ActivityHandler {
    constructor() {
        super();

        this.onMembersAdded(async (context, next) => {
            await this.sendWelcomeMessage(context);
            await next();
        });

        this.onMessage(async (context, next) => {
            const text = context.activity.text.trim();


            switch(text){
                case 'Hello':
                    await context.sendActivity('Hello! How can I assist you today?');
                    break;
                case 'Welcome':
                    await context.sendActivity('Welcome! How can I assist you today?');
                    break;
                default:
                    await context.sendActivity(`Please select one action.`);
            }

            // Send suggested actions after responding
            await this.sendSuggestedActions(context);
            await next();
        });
    }

    /**
     * Sends a welcome message along with suggested actions.
     * @param {TurnContext} turnContext - A TurnContext instance containing all the data needed for processing this conversation turn.
     */
    async sendWelcomeMessage(turnContext) {
        const { activity } = turnContext;

        // Iterate over all new members added to the conversation.
        for (const member of activity.membersAdded) {
            if (member.id !== activity.recipient.id) {
                const welcomeMessage = 'Welcome to the suggested actions bot. This bot will introduce you to suggested actions. Please select an option:';
                await turnContext.sendActivity(welcomeMessage);
                await this.sendSuggestedActions(turnContext);
            }
        }
    }

    /**
     * Sends suggested actions to the user.
     * @param {TurnContext} turnContext - A TurnContext instance containing all the data needed for processing this conversation turn.
     */
    async sendSuggestedActions(turnContext) {
        const cardActions = [
            { type: ActionTypes.ImBack, title: 'Hello', value: 'Hello' },
            { type: ActionTypes.ImBack, title: 'Welcome', value: 'Welcome' },
            {
                type: "Action.Compose",
                title: "@SuggestedActionsBot",
                value: {
                    type: "Teams.chatMessage",
                    data: {
                        body: {
                            additionalData: {},
                            backingStore: {
                                returnOnlyChangedValues: false,
                                initializationCompleted: true
                            },
                            content: "<at id=\"0\">SuggestedActionsBot</at>"
                        },
                        mentions: [
                            {
                                additionalData: {},
                                backingStore: {
                                    "returnOnlyChangedValues": false,
                                    "initializationCompleted": false
                                },
                                id: 0,
                                mentioned: {
                                    additionalData: {},
                                    backingStore: {
                                        returnOnlyChangedValues: false,
                                        initializationCompleted: false
                                    },
                                    odataType: "#microsoft.graph.chatMessageMentionedIdentitySet",
                                    user: {
                                        additionalData: {},
                                        backingStore: {
                                            returnOnlyChangedValues: false,
                                            initializationCompleted: false
                                        },
                                        displayName: "Suggested Actions Bot",
                                        id: "28:" + process.env.MICROSOFT_APP_ID,
                                    }
                                },
                                mentionText: "Suggested Actions Bot"
                            }
                        ],
                        additionalData: {},
                        backingStore: {
                            returnOnlyChangedValues: false,
                            initializationCompleted: true
                        }
                    }
                }
            }
        ];

        const reply = MessageFactory.text('Choose one of the action from the suggested action');
        reply.suggestedActions = { actions: cardActions, to: [turnContext.activity.from.id] };
        await turnContext.sendActivity(reply);
    }
}

module.exports.SuggestedActionsBot = SuggestedActionsBot;