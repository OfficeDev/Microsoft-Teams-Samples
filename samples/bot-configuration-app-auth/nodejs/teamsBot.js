// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory } = require("botbuilder");
const fs = require('fs');

/**
 * TeamsBot class extends TeamsActivityHandler to handle bot activities.
 */
class TeamsBot extends TeamsActivityHandler {
  constructor() {
    super();
    this.onMembersAdded(this.handleMembersAdded.bind(this));
    this.onMessage(this.handleMessage.bind(this));
  }

  /**
   * Handles the event when new members are added to the conversation.
   * @param {TurnContext} context - The context object for the turn.
   * @param {Function} next - The next middleware handler to call.
   */
  async handleMembersAdded(context, next) {
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
  }

  /**
   * Handles incoming messages.
   * @param {TurnContext} context - The context object for the turn.
   * @param {Function} next - The next middleware handler to call.
   */
  async handleMessage(context, next) {
    // By calling next() you ensure that the next BotHandler is run.
    await next();
  }

  /**
   * Handles the bot configuration fetch request.
   * @param {TurnContext} _context - The context object for the turn.
   * @param {Object} _configData - The configuration data.
   * @returns {Object} The response object containing the configuration.
   */
  async handleTeamsConfigFetch(_context, _configData) {
    return {
      config: {
        type: "auth",
        suggestedActions: {
          actions: [
            {
              type: "openUrl",
              value: "https://example.com/auth",
              title: "Sign in to this app"
            }
          ]
        }
      }
    };
  }

  /**
   * Handles the bot configuration submit request.
   * @param {TurnContext} context - The context object for the turn.
   * @param {Object} _configData - The configuration data.
   * @returns {Object} The response object containing the configuration.
   */
  async handleTeamsConfigSubmit(context, _configData) {
    return {
      config: {
        type: 'message',
        value: 'You have chosen to finish setting up bot',
      }
    };
  }
}

module.exports.TeamsBot = TeamsBot;