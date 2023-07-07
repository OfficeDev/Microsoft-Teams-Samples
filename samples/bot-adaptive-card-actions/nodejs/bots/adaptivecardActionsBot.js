// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
const { ActionTypes } = require('botframework-schema');

class AdaptiveCardActionsBot extends ActivityHandler {
    constructor() {
        super();

        this.onMembersAdded(async (context, next) => {
            await this.sendWelcomeMessage(context);

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });

        this.onMessage(async (context, next) => {
            const text = context.activity.text;

            if (text.includes("Card Actions")) {
                const userCard = CardFactory.adaptiveCard(this.adaptiveCardActions());
                await context.sendActivity({ attachments: [userCard] });
            }
            else if (text.includes("Suggested Actions")) {
                const userCard = CardFactory.adaptiveCard(this.SuggestedActionsCard());
                await context.sendActivity({ attachments: [userCard] });
            }
            else if (text.includes("Red") || text.includes("Blue") || text.includes("Yellow")) {
                // Create an array with the valid color options.
                const validColors = ['Red', 'Blue', 'Yellow'];

                // If the `text` is in the Array, a valid color was selected and send agreement.
                if (validColors.includes(text)) {
                    await context.sendActivity(`I agree, ${text} is the best color.`);
                }
                await this.sendSuggestedActions(context);
            }
            else if (text.includes("ToggleVisibility")) {
                const userCard = CardFactory.adaptiveCard(this.ToggleVisibleCard());
                await context.sendActivity({ attachments: [userCard] });
            }
            else {
                await context.sendActivity("Please use one of these commands: **Card Actions** for  Adaptive Card Actions, **Suggested Actions** for Bot Suggested Actions and **ToggleVisibility** for Action ToggleVisible Card");
            }
            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }

    /**
     * Send a welcome message along with suggested actions for the user to click.
     * @param {TurnContext} turnContext A TurnContext instance containing all the data needed for processing this conversation turn.
     */
    async sendWelcomeMessage(turnContext) {
        const { activity } = turnContext;

        // Iterate over all new members added to the conversation.
        for (const idx in activity.membersAdded) {
            if (activity.membersAdded[idx].id !== activity.recipient.id) {
                const welcomeMessage = `Welcome to Adaptive Card Action and Suggested Action Bot. This bot will introduce you to suggested actions.` +
                    'Please select an option:';

                await turnContext.sendActivity(welcomeMessage);
                await turnContext.sendActivity("Please use one of these commands: **1** for  Adaptive Card Actions, **2** for Bot Suggested Actions and **3** for Toggle Visible Card");
                await this.sendSuggestedActions(turnContext);
            }
        }
    }

    async sendSuggestedActions(turnContext) {
        const cardActions = [
            {
                type: ActionTypes.ImBack,
                title: 'Red',
                value: 'Red'
            },
            {
                type: ActionTypes.ImBack,
                title: 'Yellow',
                value: 'Yellow'
            },
            {
                type: ActionTypes.ImBack,
                title: 'Blue',
                value: 'Blue'
            }
        ];

        var reply = MessageFactory.text("What is your favorite color ?");
        reply.suggestedActions = { "actions": cardActions, "to": [turnContext.activity.from.id] };
        await turnContext.sendActivity(reply);
    }

    // Adaptive Card Actions
    adaptiveCardActions = () => ({
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.0",
        "body": [
            {
                "type": "TextBlock",
                "text": "Adaptive Card Actions"
            }
        ],
        "actions": [
            {
                "type": "Action.OpenUrl",
                "title": "Action Open URL",
                "url": "https://adaptivecards.io"
            },
            {
                "type": "Action.ShowCard",
                "title": "Action Submit",
                "card": {
                    "type": "AdaptiveCard",
                    "version": "1.5",
                    "body": [
                        {
                            "type": "Input.Text",
                            "id": "name",
                            "label": "Please enter your name:",
                            "isRequired": true,
                            "errorMessage": "Name is required"
                        }
                    ],
                    "actions": [
                        {
                            "type": "Action.Submit",
                            "title": "Submit"
                        }
                    ]
                }
            },
            {
                "type": "Action.ShowCard",
                "title": "Action ShowCard",
                "card": {
                    "type": "AdaptiveCard",
                    "version": "1.0",
                    "body": [
                        {
                            "type": "TextBlock",
                            "text": "This card's action will show another card"
                        }
                    ],
                    "actions": [
                        {
                            "type": "Action.ShowCard",
                            "title": "Action.ShowCard",
                            "card": {
                                "type": "AdaptiveCard",
                                "body": [
                                    {
                                        "type": "TextBlock",
                                        "text": "**Welcome To New Card**"
                                    },
                                    {
                                        "type": "TextBlock",
                                        "text": "This is your new card inside another card"
                                    }
                                ]
                            }
                        }
                    ]
                }
            }
        ]
    });

    // Toggle Visible Card
    ToggleVisibleCard = () => ({
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.0",
        "body": [
            {
                "type": "TextBlock",
                "text": "**Action.ToggleVisibility example**: click the button to show or hide a welcome message"
            },
            {
                "type": "TextBlock",
                "id": "helloWorld",
                "isVisible": false,
                "text": "**Hello World!**",
                "size": "extraLarge"
            }
        ],
        "actions": [
            {
                "type": "Action.ToggleVisibility",
                "title": "Click me!",
                "targetElements": ["helloWorld"]
            }
        ]
    })

    // Suggest Actions Card
    SuggestedActionsCard = () => ({

        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.0",
        "body": [
            {
                "type": "TextBlock",
                "text": "**Welcome to bot Suggested actions** please use below commands."
            },
            {
                "type": "TextBlock",
                "text": "please use below commands, to get response form the bot."
            },
            {
                "type": "TextBlock",
                "text": "- Red \r- Blue \r - Yellow",
                "wrap": true
            }
        ]
    })
}

module.exports.AdaptiveCardActionsBot = AdaptiveCardActionsBot;
