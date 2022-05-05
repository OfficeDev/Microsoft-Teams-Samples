const {ActivityHandler, TurnContext, CardFactory, MessageFactory } = require('botbuilder');
const { Constant } = require('../models/constant');

class ActivityBot extends ActivityHandler {
    constructor() {
        super();
        
        this.onConversationUpdate(async (context, next) => {
            // Checking if task details is present and if bot is installed.
            var isPresent = taskDetails.hasOwnProperty(Constant.TaskDetails);
            if (isPresent && context.activity.membersAdded.length > 0)
            {
                var details = taskDetails[Constant.TaskDetails];
                var cardJson = {
                    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                    "version": "1.5",
                    "type": "AdaptiveCard",
                    "body": [
                      {
                        "type": "TextBlock",
                        "text": `${details.NotificationId}: ${details.TaskTitle}`,
                        "weight": "Bolder",
                        "size": "Large"
                      },
                      {
                        "type": "TextBlock",
                        "text": "Task Details",
                        "weight": "Bolder"
                      },
                      {
                        "type": "ColumnSet",
                        "columns": [
                          {
                            "type": "Column",
                            "items": [
                              {
                                "type": "TextBlock",
                                "text": "**ID:**"
                              },
                              {
                                "type": "TextBlock",
                                "text": "**Title:**"
                              },
                              {
                                "type": "TextBlock",
                                "text": "**State:**"
                              },
                              {
                                "type": "Container",
                                "minHeight": "30px",
                                "items": [
                                  {
                                    "type": "TextBlock",
                                    "text": "**Created by:**",
                                    "height": "stretch"
                                  }
                                ]
                              },
                              {
                                "type": "TextBlock",
                                "text": "**Assigned to:**",
                                "height": "stretch"
                              },
                              {
                                "type": "TextBlock",
                                "text": "**Stakeholders:**"
                              }
                            ],
                            "width": "stretch"
                          },
                          {
                            "type": "Column",
                            "items": [
                              {
                                "type": "TextBlock",
                                "text": details.NotificationId
                              },
                              {
                                "type": "TextBlock",
                                "text": details.TaskTitle
                              },
                              {
                                "type": "TextBlock",
                                "text": details.State
                              },
                              {
                                "type": "ColumnSet",
                                "minHeight": "30px",
                                "columns": [
                                  {
                                    "type": "Column",
                                    "items": [
                                      {
                                        "type": "Image",
                                        "style": "Person",
                                        "url": details.CreatedByProfileImage,
                                        "height": "25px"
                                      }
                                    ],
                                    "width": "25px",
                                    "spacing": "None"
                                  },
                                  {
                                    "type": "Column",
                                    "items": [
                                      {
                                        "type": "TextBlock",
                                        "text": details.CreatedByName
                                      }
                                    ],
                                    "width": "stretch",
                                    "verticalContentAlignment": "Center"
                                  }
                                ]
                              },
                              {
                                "type": "ColumnSet",
                                "columns": [
                                  {
                                    "type": "Column",
                                    "items": [
                                      {
                                        "type": "Image",
                                        "style": "Person",
                                        "url": details.AssignedToProfileImage,
                                        "height": "25px"
                                      }
                                    ],
                                    "width": "25px",
                                    "spacing": "None"
                                  },
                                  {
                                    "type": "Column",
                                    "items": [
                                      {
                                        "type": "TextBlock",
                                        "text": details.AssignedToName
                                      }
                                    ],
                                    "width": "stretch",
                                    "verticalContentAlignment": "Top"
                                  }
                                ]
                              },
                              {
                                "type": "TextBlock",
                                "text": details.StakeholderTeam.join(", ")
                              }
                            ],
                            "width": "stretch"
                          }
                        ]
                      }
                    ],
                    "actions": [
                      {
                        "type": "Action.OpenUrl",
                        "title": "View",
                        "url": details.WorkitemUrl
                      }
                    ]
                };
               
                await context.sendActivity({ attachments: [CardFactory.adaptiveCard(cardJson)] });
            }
        });

        this.onMessage(async (context, next) => {
            TurnContext.removeRecipientMention(context.activity);
            const replyText = `Echo: ${ context.activity.text }`;
            await context.sendActivity(MessageFactory.text(replyText, replyText));
        });
    }
}
module.exports.ActivityBot = ActivityBot;