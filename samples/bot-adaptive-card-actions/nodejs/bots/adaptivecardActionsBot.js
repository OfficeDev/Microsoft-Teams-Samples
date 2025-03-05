// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
const { ActionTypes } = require('botframework-schema');

/**
 * AdaptiveCardActionsBot handles incoming messages and sends adaptive cards or suggested actions.
 */
class AdaptiveCardActionsBot extends ActivityHandler {
    constructor() {
        super();

        this.onMembersAdded(async (context, next) => {
            await this.sendWelcomeMessage(context);
            await next();
        });

        this.onMessage(async (context, next) => {
            const text = context.activity.text;

            if (text) {
                if (text.includes("Card Actions")) {
                    await this.sendAdaptiveCard(context, this.getAdaptiveCardActions());
                } else if (text.includes("Suggested Actions")) {
                    await this.sendAdaptiveCard(context, this.getSuggestedActionsCard());
                } else if (["Red", "Blue", "Yellow"].includes(text)) {
                    await context.sendActivity(`I agree, ${text} is the best color.`);
                    await this.sendSuggestedActions(context);
                } else if (text.includes("ToggleVisibility")) {
                    await this.sendAdaptiveCard(context, this.getToggleVisibilityCard());
                } else {
                    await context.sendActivity("Please use one of these commands: **Card Actions** for Adaptive Card Actions, **Suggested Actions** for Bot Suggested Actions, and **ToggleVisibility** for Action ToggleVisible Card");
                }
            }

            await this.handleCardActionSubmit(context);
            await next();
        });
    }

    /**
     * Sends a welcome message along with suggested actions for the user to click.
     * @param {TurnContext} context - A TurnContext instance containing all the data needed for processing this conversation turn.
     */
    async sendWelcomeMessage(context) {
        const { activity } = context;

        for (const member of activity.membersAdded) {
            if (member.id !== activity.recipient.id) {
                const welcomeMessage = `Welcome to Adaptive Card Action and Suggested Action Bot. This bot will introduce you to suggested actions. Please select an option:`;
                await context.sendActivity(welcomeMessage);
                await context.sendActivity("Please use one of these commands: **Card Actions** for Adaptive Card Actions, **Suggested Actions** for Bot Suggested Actions, and **ToggleVisibility** for Action ToggleVisible Card");
                await this.sendSuggestedActions(context);
            }
        }
    }

    /**
     * Sends the response on card action.submit.
     * @param {TurnContext} context - A TurnContext instance containing all the data needed for processing this conversation turn.
     */
    async handleCardActionSubmit(context) {
        if (context.activity.value) {
            await context.sendActivity(`Data Submitted: ${context.activity.value.name}`);
        }
    }

    /**
     * Sends suggested actions to the user.
     * @param {TurnContext} context - A TurnContext instance containing all the data needed for processing this conversation turn.
     */
    async sendSuggestedActions(context) {
        const cardActions = [
            { type: ActionTypes.ImBack, title: 'Red', value: 'Red' },
            { type: ActionTypes.ImBack, title: 'Yellow', value: 'Yellow' },
            { type: ActionTypes.ImBack, title: 'Blue', value: 'Blue' }
        ];

        const reply = MessageFactory.text("What is your favorite color?");
        reply.suggestedActions = { actions: cardActions, to: [context.activity.from.id] };
        await context.sendActivity(reply);
    }

    /**
     * Sends an adaptive card to the user.
     * @param {TurnContext} context - A TurnContext instance containing all the data needed for processing this conversation turn.
     * @param {Object} card - The adaptive card to send.
     */
    async sendAdaptiveCard(context, card) {
        const userCard = CardFactory.adaptiveCard(card);
        await context.sendActivity({ attachments: [userCard] });
    }

    /**
     * Returns the adaptive card actions.
     * @returns {Object} The adaptive card actions.
     */
    getAdaptiveCardActions() {
        return {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.0",
            "body": [{ "type": "TextBlock", "text": "Adaptive Card Actions" }],
            "actions": [
                { "type": "Action.OpenUrl", "title": "Action Open URL", "url": "https://adaptivecards.io" },
                {
                    "type": "Action.ShowCard", "title": "Action Submit", "card": {
                        "type": "AdaptiveCard", "version": "1.5",
                        "body": [{ "type": "Input.Text", "id": "name", "label": "Please enter your name:", "isRequired": true, "errorMessage": "Name is required" }],
                        "actions": [{ "type": "Action.Submit", "title": "Submit" }]
                    }
                },
                {
                    "type": "Action.ShowCard", "title": "Action ShowCard", "card": {
                        "type": "AdaptiveCard", "version": "1.0",
                        "body": [{ "type": "TextBlock", "text": "This card's action will show another card" }],
                        "actions": [{
                            "type": "Action.ShowCard", "title": "Action.ShowCard", "card": {
                                "type": "AdaptiveCard",
                                "body": [
                                    { "type": "TextBlock", "text": "**Welcome To New Card**" },
                                    { "type": "TextBlock", "text": "This is your new card inside another card" }
                                ]
                            }
                        }]
                    }
                }
            ]
        };
    }

    /**
     * Returns the toggle visibility card.
     * @returns {Object} The toggle visibility card.
     */
    getToggleVisibilityCard() {
        return {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.0",
            "body": [
                { "type": "TextBlock", "text": "**Action.ToggleVisibility example**: click the button to show or hide a welcome message" },
                { "type": "TextBlock", "id": "helloWorld", "isVisible": false, "text": "**Hello World!**", "size": "extraLarge" }
            ],
            "actions": [{ "type": "Action.ToggleVisibility", "title": "Click me!", "targetElements": ["helloWorld"] }]
        };
    }

    /**
     * Returns the suggested actions card.
     * @returns {Object} The suggested actions card.
     */
    getSuggestedActionsCard() {
        return {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.0",
            "body": [
                { "type": "TextBlock", "text": "**Welcome to bot Suggested actions** please use below commands." },
                { "type": "TextBlock", "text": "please use below commands, to get response form the bot." },
                { "type": "TextBlock", "text": "- Red \r- Blue \r - Yellow", "wrap": true }
            ]
        };
    }
}

module.exports.AdaptiveCardActionsBot = AdaptiveCardActionsBot;
