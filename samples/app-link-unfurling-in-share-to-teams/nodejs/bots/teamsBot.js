// <copyright file="teamsBot.js" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

const { TeamsActivityHandler, CardFactory } = require("botbuilder");

class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
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
          "version": "1.4",
          "body": [
            {
              "type": "TextBlock",
              "size": "Medium",
              "weight": "Bolder",
              "text": "Analytics details:"
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
                "url": `https://teams.microsoft.com/l/entity/${process.env.MicrosoftAppId}/tab?webUrl=${process.env.ApplicationBaseUrl}/tab?openInTeams=true`
              },
              {
                "type": "Action.Submit",
                "title": "View via card",
                "data": {
                    "msteams": {
                        "type": "invoke",
                        "value": {
                            "type": "tab/tabInfoAction",
                            "tabInfo": {
                                "contentUrl":  process.env.ApplicationBaseUrl + "/tab?openInTeams=true",
                                "websiteUrl": process.env.ApplicationBaseUrl + "/tab?openInTeams=true",
                                "name": "Stage view",
                                "entityId": "entityId"
                             }
                            }
                        }
                }
            }
          ]
        }
      
        return card;
      }
}

module.exports.TeamsBot = TeamsBot;