// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler , CardFactory } = require('botbuilder');
const adaptiveCards = require('../models/adaptiveCard');

class BotActivityHandler extends TeamsActivityHandler  {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            const replyText = `Echo: ${ context.activity.text }`;  
            var AdaptiveCardDeepLink = adaptiveCards.getDeepLinkTabStatic("deepLinkTab",3,"Adaptive Card",process.env.MicrosoftAppId);  
            var link = "https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/deep-links"
            await context.sendActivity({ attachments: [CardFactory.adaptiveCard(adaptiveCards.adaptiveCardWithLink(link))] });
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