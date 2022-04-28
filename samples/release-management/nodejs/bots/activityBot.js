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
                            "text": `Welcome to the group chat for ${details.TaskTitle}`,
                            "weight": "bolder",
                            "size": "large",
                            "wrap": true
                        },
                        {
                            "type": "TextBlock",
                            "text": "Task Details",
                            "weight": "Bolder"
                        },
                        {
                            "type": "FactSet",
                            "facts": [
                                {
                                    "title": "Title:",
                                    "value": details.TaskTitle
                                },
                                {
                                    "title": "State:",
                                    "value": details.State
                                },
                                {
                                    "title": "Created by:",
                                    "value": details.CreatedByName
                                },
                                {
                                    "title": "Assigned to:",
                                    "value": details.AssignedToName
                                },
                                {
                                    "title": "Stakeholders:",
                                    "value": details.StakeholderTeam.join(", ")
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