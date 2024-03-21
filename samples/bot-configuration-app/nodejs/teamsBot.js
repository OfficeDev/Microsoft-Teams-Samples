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
    const cardJson = {
      type: 'AdaptiveCard',
      version: '1.4',
      body: [
        {
          type: 'TextBlock',
          text: 'Bot Config Fetch',
        },
      ],
    };
    const card = CardFactory.adaptiveCard(cardJson);
    /*
    Option 1: You can add a "config/auth" response as below code
    Note: The URL in value must be linked to a valid auth URL which can be opened in a browser. This code is only representative and not a working example.
    */
    /*response = {
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
    };*/

    /*
      Option 2: You can add a "config/continue" response as below code
      */
    const adaptiveCard = CardFactory.adaptiveCard(this.adaptiveCardForContinue());
    response = {
      config: {
        value: {
          card: adaptiveCard,
          height: 200,
          width: 200,
          title: 'test card',
        },
        type: 'continue',
      },
    };
    return response;
  }


  async handleTeamsConfigSubmit(context, _configData) {
    let response = {};
    const choice = context._activity.value.data.choiceselect;
    if (choice === "continue") {
      const adaptiveCard = CardFactory.adaptiveCard(this.adaptiveCardForSubmit());
      response = {
        config: {
          type: 'continue',
          value: {
            card: adaptiveCard,
            height: 200,
            width: 200,
            title: 'Task module submit response',
          }
        },
      };
      return response;
    }
    else {
      response = {
        config: {
          type: 'message',
          value: 'You have chosen to finish setting up bot',
        },
      }
      return response;

    }
  }

  adaptiveCardForContinue = () => ({
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.2",
    "type": "AdaptiveCard",
    "body": [
      {
        "text": "Please choose bot set up option",
        "wrap": true,
        "type": "TextBlock"
      },
      {
        "columns": [
          {
            "width": "auto",
            "items": [
              {
                "text": "Option: ",
                "wrap": true,
                "height": "stretch",
                "type": "TextBlock"
              }
            ],
            "type": "Column"
          }
        ],
        "type": "ColumnSet"
      },
      {
        "columns": [
          {
            "width": "stretch",
            "items": [
              {
                "choices": [
                  {
                    "title": "Continue with more options",
                    "value": "continue"
                  },
                  {
                    "title": "Finish setting up bot",
                    "value": "finish"
                  }
                ],
                "style": "filtered",
                "placeholder": "Search for an option",
                "id": "choiceselect",
                "type": "Input.ChoiceSet"
              }
            ],
            "type": "Column"
          }
        ],
        "type": "ColumnSet"
      }
    ],
    "actions": [
      {
        "type": "Action.Submit",
        "id": "submit",
        "title": "Submit"
      }
    ]
  });

  adaptiveCardForSubmit = () => ({
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.2",
    "type": "AdaptiveCard",
    "body": [
      {
        "text": "Please hit submit to continue setting up bot",
        "wrap": true,
        "type": "TextBlock"
      }
    ],
    "actions": [
      {
        "type": "Action.Submit",
        "id": "submitdynamic",
        "title": "Submit"
      }
    ]
  });
}

module.exports.TeamsBot = TeamsBot;
