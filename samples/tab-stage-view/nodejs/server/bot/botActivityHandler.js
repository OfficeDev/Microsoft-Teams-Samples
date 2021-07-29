// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler } = require('botbuilder');
const adaptiveCards = require('../models/adaptiveCard');
const { GraphClient } = require('../graphClient')

class BotActivityHandler extends TeamsActivityHandler {
    constructor() {
        super();
    }

    async onMessageActivity(context) {
        console.log('Activity: ', context.activity.name);

        await context.sendActivity({
            attachments: [CardFactory.adaptiveCard(adaptiveCards.adaptiveCardWithLink())]
        });
}
}

module.exports.BotActivityHandler = BotActivityHandler;