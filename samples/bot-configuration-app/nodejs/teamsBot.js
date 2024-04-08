// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory } = require("botbuilder");
const fs = require('fs');
const AdaptiveCard = require('adaptivecards');
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
    const adaptiveCard = CardFactory.adaptiveCard(this.adaptiveCardForContinue());
    response = {
      config: {
        value: {
          card: adaptiveCard,
          height: 500,
          width: 600,
          title: 'test card',
        },
        type: 'continue',
      },
    };
    return response;
  }


  async handleTeamsConfigSubmit(context, _configData) {
    let response = {};
    const data = context._activity.value.data;
    const dropdown01Value = data?.dropdown01;
    const dropdown02Value = data?.dropdown02;
    const dropdown1Value = data?.dropdown1;
    const dropdown2Value = data?.dropdown2;
    const dropdown3Value = data?.dropdown3;
    const dropdown4Value = data?.dropdown4;
    const togglestatus = data?.togglestatus;
    const toggleAssign = data?.toggleAssign;
    const toggleComment = data?.toggleComment;
    const toggleTransition = data?.toggleTransition;

    const card = {
      $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
      version: '1.2',
      type: 'AdaptiveCard',
      body: [
        {
          type: 'TextBlock',
          text: 'The selection you requested is as follows:',
          weight: 'bolder',
          wrap: true
        }
      ]
    };

    if (dropdown01Value) {
      card.body.push({
        type: 'TextBlock',
        text: `Type : ${dropdown01Value}`,
        wrap: true
      });
    }

    if (dropdown02Value) {
      card.body.push({
        type: 'TextBlock',
        text: `Priority : ${dropdown02Value}`,
        wrap: true
      });
    }

    if (dropdown1Value) {
      card.body.push({
        type: 'TextBlock',
        text: `Issue : ${dropdown1Value}`,
        wrap: true
      });
    }

    if (dropdown2Value) {
      card.body.push({
        type: 'TextBlock',
        text: `Comment : ${dropdown2Value}`,
        wrap: true
      });
    }

    if (dropdown3Value) {
      card.body.push({
        type: 'TextBlock',
        text: `Assignee : ${dropdown3Value}`,
        wrap: true
      });
    }

    if (dropdown4Value) {
      card.body.push({
        type: 'TextBlock',
        text: `Status : ${dropdown4Value}`,
        wrap: true
      });
    }

    card.body.push({
      type: 'TextBlock',
      text: 'Actions to be displayed:',
      weight: 'bolder',
      wrap: true
    });

    if (togglestatus === 'true') {
      card.body.push({
        type: 'TextBlock',
        text: `Status : ${togglestatus}`,
        wrap: true
      });
    }

    if (toggleAssign === 'true') {
      card.body.push({
        type: 'TextBlock',
        text: `Assign : ${toggleAssign}`,
        wrap: true
      });
    }

    if (toggleComment === 'true') {
      card.body.push({
        type: 'TextBlock',
        text: `Comment : ${toggleComment}`,
        wrap: true
      });
    }

    if (toggleTransition === 'true') {
      card.body.push({
        type: 'TextBlock',
        text: `Transition : ${toggleTransition}`,
        wrap: true
      });
    }

    const attachment = CardFactory.adaptiveCard(card);
    const reply = { type: 'message', attachments: [attachment] };
    await context.sendActivity(reply);

    response = {
      config: {
        type: 'message',
        value: 'Your request has been submitted successfully!',
      },
    }
    return response;
  }

  adaptiveCardForContinue = () => ({
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.4",
    "type": "AdaptiveCard",
    "body": [
      {
        "type": "ColumnSet",
        "columns": [
          {
            "type": "Column",
            "width": "stretch",
            "items": [
              {
                "type": "TextBlock",
                "text": "For issues that match these criteria:",
                "weight": "bolder"
              }
            ]
          }
        ]
      },
      {
        "type": "ColumnSet",
        "columns": [
          {
            "type": "Column",
            "width": "stretch",
            "items": [
              {
                "type": "TextBlock",
                "text": "Type",
                "weight": "bolder"
              },
              {
                "type": "Input.ChoiceSet",
                "id": "dropdown01",
                "choices": [
                  {
                    "title": "Bug",
                    "value": "Bug"
                  },
                  {
                    "title": "Feature Request",
                    "value": "Feature Request"
                  },
                  {
                    "title": "Task",
                    "value": "Task"
                  }
                ]
              }
            ]
          },
          {
            "type": "Column",
            "width": "stretch",
            "items": [
              {
                "type": "TextBlock",
                "text": "Priority",
                "weight": "bolder"
              },
              {
                "type": "Input.ChoiceSet",
                "id": "dropdown02",
                "choices": [
                  {
                    "title": "Low",
                    "value": "Low"
                  },
                  {
                    "title": "Medium",
                    "value": "Medium"
                  },
                  {
                    "title": "High",
                    "value": "High"
                  }
                ]
              }
            ]
          }
        ]
      },
      {
        "type": "ColumnSet",
        "columns": [
          {
            "type": "Column",
            "width": "stretch",
            "items": [
              {
                "type": "TextBlock",
                "text": "Post to channel when :",
                "weight": "bolder"
              }
            ]
          }
        ]
      },
      {
        "type": "ColumnSet",
        "columns": [
          {
            "type": "Column",
            "width": "stretch",
            "items": [
              {
                "type": "TextBlock",
                "text": "Issue",
                "weight": "bolder"
              },
              {
                "type": "Input.ChoiceSet",
                "id": "dropdown1",
                "isMultiSelect": true,
                "choices": [
                  {
                    "title": "Software Issue",
                    "value": "Software Issue"
                  },
                  {
                    "title": "Server Issue",
                    "value": "Server Issue"
                  },
                  {
                    "title": "Network Issue",
                    "value": "Network Issue"
                  }
                ]
              }
            ]
          },
          {
            "type": "Column",
            "width": "stretch",
            "items": [
              {
                "type": "TextBlock",
                "text": "Comment",
                "weight": "bolder"
              },
              {
                "type": "Input.ChoiceSet",
                "id": "dropdown2",
                "choices": [
                  {
                    "title": "Network problem in server",
                    "value": "Network problem in server"
                  },
                  {
                    "title": "Loadbalancer issue",
                    "value": "Loadbalancer issue"
                  },
                  {
                    "title": "Software needs to be updated",
                    "value": "Software needs to be updated"
                  }
                ]
              }
            ]
          }
        ]
      },
      {
        "type": "ColumnSet",
        "columns": [
          {
            "type": "Column",
            "width": "stretch",
            "items": [
              {
                "type": "TextBlock",
                "text": "Assignee",
                "weight": "bolder"
              },
              {
                "type": "Input.ChoiceSet",
                "id": "dropdown3",
                "choices": [
                  {
                    "title": "Jasmine Smith",
                    "value": "Jasmine Smith"
                  },
                  {
                    "title": "Ethan Johnson",
                    "value": "Ethan Johnson"
                  },
                  {
                    "title": "Maya Rodriguez",
                    "value": "Maya Rodriguez"
                  }
                ]
              }
            ]
          },
          {
            "type": "Column",
            "width": "stretch",
            "items": [
              {
                "type": "TextBlock",
                "text": "Status changed",
                "weight": "bolder"
              },
              {
                "type": "Input.ChoiceSet",
                "id": "dropdown4",
                "choices": [
                  {
                    "title": "Open",
                    "value": "Open"
                  },
                  {
                    "title": "Inprogress",
                    "value": "Inprogress"
                  },
                  {
                    "title": "Completed",
                    "value": "Completed"
                  }
                ]
              }
            ]
          }
        ]
      },
      {
        "type": "ColumnSet",
        "columns": [
          {
            "type": "Column",
            "width": "stretch",
            "items": [
              {
                "type": "TextBlock",
                "text": "Actions to display",
                "weight": "bolder"
              }
            ]
          }
        ]
      },
      {
        "type": "ColumnSet",
        "columns": [
          {
            "type": "Column",
            "width": "stretch",
            "items": [
              {
                "type": "Input.Toggle",
                "title": "Assign",
                "id": "toggleAssign",
                "value": "false"
              }
            ]
          }
        ]
      },
      {
        "type": "ColumnSet",
        "columns": [
          {
            "type": "Column",
            "width": "stretch",
            "items": [
              {
                "type": "Input.Toggle",
                "title": "Comment",
                "id": "toggleComment",
                "value": "false"
              }
            ]
          }
        ]
      },
      {
        "type": "ColumnSet",
        "columns": [
          {
            "type": "Column",
            "width": "stretch",
            "items": [
              {
                "type": "Input.Toggle",
                "title": "Transition",
                "id": "toggleTransition",
                "value": "false"
              }
            ]
          }
        ]
      },
      {
        "type": "ColumnSet",
        "columns": [
          {
            "type": "Column",
            "width": "stretch",
            "items": [
              {
                "type": "Input.Toggle",
                "title": "Update status",
                "id": "togglestatus",
                "value": "false"
              }
            ]
          }
        ]
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
