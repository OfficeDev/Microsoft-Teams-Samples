// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    TurnContext,
    MessageFactory,
    TeamsActivityHandler
} = require('botbuilder');

const { GetTranslatedRes} = require('../services/languageService')

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
        // Registers an activity event handler for the message event, emitted for every incoming message activity.
        this.onMessage(async (context, next) => {
            TurnContext.removeRecipientMention(context.activity);
            const locale = context.activity.locale
            const text = GetTranslatedRes(locale).welcome
            await this.replyActivityAsync(context, text)
            await next();
        });
    }

    async replyActivityAsync(context, text) {
        const replyActivity = MessageFactory.text(text);        
        await context.sendActivity(replyActivity);
    }
}

module.exports.BotActivityHandler = BotActivityHandler;

