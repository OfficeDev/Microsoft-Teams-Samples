// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler , CardFactory, MessageFactory } = require('botbuilder');
const adaptiveCards = require('../models/adaptiveCard');

class BotActivityHandler extends TeamsActivityHandler  {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            // Base url without protocol to be used in OpenUrl encoded deeplink
            var baseUrl = process.env.baseUrl ? process.env.baseUrl.split(':')[1] : '';
            await context.sendActivity({ attachments: [CardFactory.adaptiveCard(adaptiveCards.adaptiveCardForTabStageView(baseUrl))] });
            await next();
        });

        this.onMembersAdded(async (context, next) => {
          var welcomeText = "Hello and welcome!, Please type any bot command to see the stage view feature";
          await context.sendActivity(MessageFactory.text(welcomeText));
          await next();
      });
    }
}

module.exports.BotActivityHandler = BotActivityHandler;