// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory, ActivityHandler } = require('botbuilder');
const AdaptiveCardResponse = require('../resources/adaptiveCardResponseJson.json');
const Options = require('../resources/options.json');

class BotSSOAdativeCard extends ActivityHandler {
    constructor() {
        super();

        this.onMembersAdded(async (context, next) => {
            //Send welcome message when app installed
            await this.sendWelcomeMessage(context);

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });

        this.onMessage(async (context, next) => {
            const text = context.activity.text;

            if (text) {
                switch (text) {
                    case "login":
                        await context.sendActivity({ attachments: [this.getAdaptiveCardWithAuthOptions()] });
                        break;

                    case "PerformSSO":
                        await context.sendActivity({ attachments: [this.getAdaptiveCardResponse()] });
                        break;

                    default:
                        const message = "Please send 'login' for options";
                        await context.sendActivity(message);
                        break;
                }
            }

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }

    /**
     * Send a welcome message.
     * @param {TurnContext} turnContext A TurnContext instance containing all the data needed for processing this conversation turn.
     */
    async sendWelcomeMessage(turnContext) {
        const { activity } = turnContext;
        // Iterate over all new members added to the conversation.
        for (const id in activity.membersAdded) {
            if (activity.membersAdded[id].id !== activity.recipient.id) {
                const welcomeMessage = `Welcome to Universal Adaptive Cards. Type 'login' to get sign in universal sso.`;

                await turnContext.sendActivity(welcomeMessage);
            }
        }
    }

    /**
    * Send a adaptive card of with actions. 
    */
    getAdaptiveCardWithAuthOptions() {
        return CardFactory.adaptiveCard(Options);
    }

    /**
    * Send a adaptive card response for successfull login.
    */
    getAdaptiveCardResponse() {
        return CardFactory.adaptiveCard(AdaptiveCardResponse);
    }
}

module.exports.BotSSOAdativeCard = BotSSOAdativeCard;
