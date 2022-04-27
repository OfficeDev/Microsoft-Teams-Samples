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
                var cardJson = {
                    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                    "version": "1.4",
                    "type": "AdaptiveCard",
                    "body": [
                        {
                            "type": "TextBlock",
                            "text": "New task created",
                            "weight": "bolder"
                        },
                        {
                            "type": "TextBlock",
                            "text": taskDetails[Constant.TaskDetails].TaskTitle,
                        },
                        {
                            "type": "TextBlock",
                            "text": `Assigned to- ${taskDetails[Constant.TaskDetails].AssignedToName}`
                        },
                        {
                            "type": "TextBlock",
                            "text": `State- ${taskDetails[Constant.TaskDetails].State}`,
                        }
                    ]
                }
               
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