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

            // Valid color options
            const validColors = ['Red', 'Blue', 'Yellow'];

            // Check if the text is a valid color
            if (validColors.includes(text)) {
                await context.sendActivity(`I agree, ${text} is the best color.`);
            } else {
                await context.sendActivity('Please select a color.');
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
            { type: ActionTypes.ImBack, title: 'Red', value: 'Red' },
            { type: ActionTypes.ImBack, title: 'Yellow', value: 'Yellow' },
            { type: ActionTypes.ImBack, title: 'Blue', value: 'Blue' }
        ];

        const reply = MessageFactory.text('What is your favorite color?');
        reply.suggestedActions = { actions: cardActions, to: [turnContext.activity.from.id] };
        await turnContext.sendActivity(reply);
    }
}

module.exports.SuggestedActionsBot = SuggestedActionsBot;