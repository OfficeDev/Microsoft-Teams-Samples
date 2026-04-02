// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    TurnContext,
    MessageFactory,
    TeamsActivityHandler
} = require('botbuilder');

const { GetTranslatedRes } = require('../services/languageService');

/**
 * BotActivityHandler handles incoming messages from users.
 * It processes the incoming messages and sends a reply in the appropriate language.
 */
class BotActivityHandler extends TeamsActivityHandler {
    constructor() {
        super();

        /*  Teams bots are Microsoft Bot Framework bots.
           If a bot receives a message activity, the turn handler sees that incoming activity
           and sends it to the onMessage activity handler.
           Learn more: https://aka.ms/teams-bot-basics.

           NOTE:   Ensure the bot endpoint that services incoming conversational bot queries is
                   registered with Bot Framework.
                   Learn more: https://aka.ms/teams-register-bot. 
       */
        // Register onMessage event handler for processing incoming messages
        this.onMessage(async (context, next) => {
            try {
                // Remove the mention of the bot in the received message
                TurnContext.removeRecipientMention(context.activity);

                // Get the locale of the activity (user's language preference)
                const locale = context.activity.locale || 'en-us'; // Default to 'en-us' if locale is not provided

                // Get the translation for the welcome message based on locale
                const text = GetTranslatedRes(locale).welcome;

                // Send the welcome message to the user
                await this.replyActivityAsync(context, text);

                // Continue processing any subsequent handlers
                await next();
            } catch (error) {
                // Handle errors gracefully (e.g., missing translation files)
                console.error(`Error handling message: ${error.message}`);
                const defaultText = GetTranslatedRes('en-us').welcome; // Fallback to English if an error occurs
                await this.replyActivityAsync(context, defaultText);
            }
        });
    }

    /**
     * Sends a reply activity to the user.
     * @param {TurnContext} context - The context of the current conversation turn.
     * @param {string} text - The text to send as a response.
     */
    async replyActivityAsync(context, text) {
        const replyActivity = MessageFactory.text(text);
        await context.sendActivity(replyActivity);
    }
}

module.exports.BotActivityHandler = BotActivityHandler;
