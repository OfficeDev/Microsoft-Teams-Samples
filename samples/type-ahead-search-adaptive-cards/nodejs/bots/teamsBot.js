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
                    await context.sendActivity("Hello and welcome! With this sample you can checkin your location (use command 'checkin') and view your checked in location(use command 'viewcheckin').");
                }
            }

            await next();
        });

        this.onMessage(async (context, next) => {

            const userCard = CardFactory.adaptiveCard(this.adaptiveCardForTaskModule());
            await context.sendActivity({ attachments: [userCard] });

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }

    adaptiveCardForTaskModule = () => ({
        "type": "AdaptiveCard",
        "body": [
          {
            "columns": [
              {
                "width": "1",
                "items": [
                  {
                    "size": null,
                    "url": "https://urlp.asm.skype.com/v1/url/content?url=https%3a%2f%2fi.imgur.com%2fhdOYxT8.png",
                    "height": "auto",
                    "type": "Image"
                  }
                ],
                "type": "Column"
              },
              {
                "width": "2",
                "items": [
                  {
                    "size": "extraLarge",
                    "text": "Game Purchase",
                    "weight": "bolder",
                    "wrap": true,
                    "type": "TextBlock"
                  }
                ],
                "type": "Column"
              }
            ],
            "type": "ColumnSet"
          },
          {
            "text": "Please fill out the below form to send a game purchase request.",
            "wrap": true,
            "type": "TextBlock"
          },
          {
            "columns": [
              {
                "width": "auto",
                "items": [
                  {
                    "text": "Game: ",
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
                        "title": "Call of Duty",
                        "value": "call_of_duty"
                      },
                      {
                        "title": "Death's Door",
                        "value": "deaths_door"
                      },
                      {
                        "title": "Grand Theft Auto V",
                        "value": "grand_theft"
                      },
                      {
                        "title": "Minecraft",
                        "value": "minecraft"
                      }
                    ],
                    "style": "filtered",
                    "placeholder": "Search for a game",
                    "id": "choiceGameSingle",
                    "type": "Input.ChoiceSet"
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
                "width": "auto",
                "items": [
                  {
                    "text": "Multi-Game: ",
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
                        "title": "Static Option 1",
                        "value": "static_option_1"
                      },
                      {
                        "title": "Static Option 2",
                        "value": "static_option_2"
                      },
                      {
                        "title": "Static Option 3",
                        "value": "static_option_3"
                      }
                    ],
                    "isMultiSelect": true,
                    "style": "filtered",
                    "choices.data": {
                      "type": "Data.Query",
                      "dataset": "xbox"
                    },
                    "id": "choiceGameMulti",
                    "type": "Input.ChoiceSet"
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
                "width": "auto",
                "items": [
                  {
                    "text": "Needed by: ",
                    "wrap": true,
                    "height": "stretch",
                    "type": "TextBlock"
                  }
                ],
                "type": "Column"
              },
              {
                "width": "stretch",
                "items": [
                  {
                    "id": "choiceDate",
                    "type": "Input.Date"
                  }
                ],
                "type": "Column"
              }
            ],
            "type": "ColumnSet"
          },
          {
            "text": "Buy and download digital games and content directly from your Xbox console, Windows 10 PC, or at Xbox.com.",
            "wrap": true,
            "type": "TextBlock"
          },
          {
            "text": "Earn points for what you already do on Xbox, then redeem your points on real rewards. Play more, get rewarded. Start earning today.",
            "wrap": true,
            "type": "TextBlock"
          }
        ],
        "actions": [
          {
            "data": {
              "msteams": {
                "type": "invoke",
                "value": {
                  "type": "task/submit"
                }
              }
            },
            "title": "Request Purchase",
            "type": "Action.Submit"
          }
        ],
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.2"
      });

    adaptiveCardForUserLastCheckin = (userDetail) => ({
        $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
        body: [
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: `User name: ${userDetail.userName}`
            },
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                wrap: "true",
                text: `Check in time: ${userDetail.time}`
            },
            {
                type: 'ActionSet',
                actions: [
                    {
                        type: "Action.Submit",
                        title: "View Location",
                        data: {
                            msteams: {
                                type: "task/fetch"
                            },
                            id: "viewLocation",
                            latitude: userDetail.latitude,
                            longitude: userDetail.longitude
                        }
                    }
                ]
            }
        ],
        type: "AdaptiveCard",
        version: "1.2"
    });

}

module.exports.TeamsBot = TeamsBot;