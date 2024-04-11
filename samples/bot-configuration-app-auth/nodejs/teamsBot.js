// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory } = require("botbuilder");
const fs = require('fs');
class TeamsBot extends TeamsActivityHandler {
  constructor() {
    super();
    this.onMembersAdded(async (context, next) => {
      const imagePath = 'Images/configbutton.png';
      const imageBase64 = fs.readFileSync(imagePath, 'base64');
      const card = CardFactory.adaptiveCard({
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.0",
        "body": [
          {
            "type": "TextBlock",
            "text": "Hello and welcome! With this sample, you can experience the functionality of bot configuration. To access Bot configuration, click on the settings button in the bot description card.",
            "wrap": true,
            "size": "large",
            "weight": "bolder"
          },
          {
            "type": "Image",
            "url": `data:image/jpeg;base64,${imageBase64}`,
            "size": "auto"
          }
        ],
        "fallbackText": "This card requires Adaptive Card support."
      });

      await context.sendActivity({
        text: '',
        attachments: [card]
      });

      await next();
    });

    this.onMessage(async (context, next) => {

      // By calling next() you ensure that the next BotHandler is run.
      await next();
    });
  }

  /* Implementing sdk handler for bot configuration
  configData object currently doesnt suport any data
  */
  async handleTeamsConfigFetch(_context, _configData) {
    let response = {};

    response = {
      config: {
        type: "auth",
        suggestedActions: {
          actions: [
            {
              type: "openUrl",
              value: "https://example.com/auth",
              title: "Sign in to this app"
            }]
        },
      },
    };

    return response;
  }


  async handleTeamsConfigSubmit(context, _configData) {
    let response = {};
    
    response = {
      config: {
        type: 'message',
        value: 'You have chosen to finish setting up bot',
      },
    }
    return response;
  }
}

module.exports.TeamsBot = TeamsBot;
