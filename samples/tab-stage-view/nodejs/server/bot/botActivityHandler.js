// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler , CardFactory, MessageFactory } = require('botbuilder');
const adaptiveCards = require('../models/adaptiveCard');

class BotActivityHandler extends TeamsActivityHandler  {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            await context.sendActivity({ attachments: [CardFactory.adaptiveCard(adaptiveCards.adaptiveCardWithLink())] });
            await next();
        });

        this.onMembersAdded(async (context, next) => {
          var welcomeText = "Hello and welcome!, Please type any bot command to see the stage view feature";
          await context.sendActivity(MessageFactory.text(welcomeText));
          await next();
      });
    }

    handleTeamsAppBasedLinkQuery(context, query) {
        const attachment = CardFactory.thumbnailCard('Thumbnail Card',
          query.url,
          ['https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png']);
    
        const result = {
          attachmentLayout: 'list',
          type: 'result',
          attachments: [CardFactory.adaptiveCard(adaptiveCards.adaptiveCardWithLink(query.url))]
        };
    
        const response = {
          composeExtension: result
        };
        return response;
      }
   
}

module.exports.BotActivityHandler = BotActivityHandler;