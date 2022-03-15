// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory } = require("botbuilder");

class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let member = 0; member < membersAdded.length; member++) {
                if (membersAdded[member].id !== context.activity.recipient.id) {
                    await context.sendActivity("Hello and welcome! This sample demonstrates the use of link unfurling in share to teams scope.");
                }
            }

            await next();
        });
    }

    // Invoked when an app based link query activity is received from the connector.
    async handleTeamsAppBasedLinkQuery(context, query) {
        var userCard = CardFactory.adaptiveCard(this.getLinkUnfurlingCard());
        const preview = CardFactory.thumbnailCard(
            'Adaptive Card',
            'Please select to get the card'
            );

        return {
            composeExtension: {
                attachmentLayout: 'list',
                type: 'result',
                attachments: [{...userCard,preview}]
            }
        }
    }

    // Adaptive card for link unfurling.
    getLinkUnfurlingCard = () => {
        var card = {
          "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
          "type": "AdaptiveCard",
          "version": "1.0",
          "body": [
            {
              "type": "TextBlock",
              "size": "Medium",
              "weight": "Bolder",
              "text": "The analytics details are"
            },
            {
              "type": "Image",
              "url": `${process.env.ApplicationBaseUrl}/Images/report.png`
            }
          ],
          "actions":[
            {
                "type": "Action.OpenUrl",
                "title": "Open tab",
                "url": `https://teams.microsoft.com/l/entity/${process.env.MicrosoftAppId}/tab?webUrl=${process.env.ApplicationBaseUrl}/AuthTab`
              }
          ]
        }
      
        return card;
      }
}

module.exports.TeamsBot = TeamsBot;