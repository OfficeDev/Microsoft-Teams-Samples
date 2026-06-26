const { TurnContext, CardFactory, MessageFactory, TeamsInfo, TeamsActivityHandler } = require('botbuilder');
const GraphHelper = require('../helpers/graphHelper');

class ActivityBot extends TeamsActivityHandler {
    constructor() {
        super();

        // Activity handler for message event.
        this.onMessage(async (context, next) => {
            TurnContext.removeRecipientMention(context.activity);
            const replyText = `Echo: ${ context.activity.text }`;
            await context.sendActivity(MessageFactory.text(replyText, replyText));
        });

        // Activity handler for task module fetch event.
        this.handleTeamsTaskModuleFetch = async (context, taskModuleRequest) => {
          try {
            var meetingId = taskModuleRequest.data.meetingId;

            return {
                "task": {
                    "type": "continue",
                    "value": {
                        "title": "Meeting Transcript",
                        "height": 600,
                        "width": 600,
                        "url": `${process.env.AppBaseUrl}/home?meetingId=${meetingId}`,
                    },
                },
            };
          }
          catch (ex) {

            return {
                "task": {
                    "type": "continue",
                    "value": {
                        "title": "Testing",
                        "height": 600,
                        "width": 600,
                        "url": `${process.env.AppBaseUrl}/home` ,
                    },
                },
            };
          }
        }

        // Activity handler for meeting end event.
        this.onTeamsMeetingEndEvent(async (meeting, context, next) => {
          var meetingDetails = await TeamsInfo.getMeetingInfo(context);
          var graphHelper = new GraphHelper();

          var result = await graphHelper.GetMeetingTranscriptionsAsync(meetingDetails.details.msGraphResourceId);
          if (result != "")
          {
            result = result.replace("<v", "");
            var foundIndex = transcriptsDictionary.findIndex((x) => x.id === meetingDetails.details.msGraphResourceId);
            
            if (foundIndex != -1) {
              transcriptsDictionary[foundIndex].data = result;
            }
            else {
              transcriptsDictionary.push({
                id: meetingDetails.details.msGraphResourceId,
                data: result
              });
            }

            var cardJson = {
              "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
              "version": "1.5",
              "type": "AdaptiveCard",
              "body": [
                {
                  "type": "TextBlock",
                  "text": "Here is the last transcript details of the meeting.",
                  "weight": "Bolder",
                  "size": "Large"
                }
              ],
              "actions": [
                {
                  "type": "Action.Submit",
                  "title": "View Transcript",
                  "data": {
                    "msteams": {
                      "type": "task/fetch"
                    },
                    "meetingId": meetingDetails.details.msGraphResourceId
                  }
                }
              ]
            };

            await context.sendActivity({ attachments: [CardFactory.adaptiveCard(cardJson)] });
          }
          else
          {
              var notFoundCardJson = {
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.5",
                "type": "AdaptiveCard",
                "body": [
                  {
                    "type": "TextBlock",
                    "text": "Transcript not found for this meeting.",
                    "weight": "Bolder",
                    "size": "Large"
                  }
                ]
              };
              
            await context.sendActivity({ attachments: [CardFactory.adaptiveCard(notFoundCardJson)] });
          }
        });
    }
}
module.exports.ActivityBot = ActivityBot;