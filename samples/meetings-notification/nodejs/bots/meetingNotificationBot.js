// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, MessageFactory, CardFactory, TeamsInfo, TurnContext } = require('botbuilder');
const { MicrosoftAppCredentials } = require('botframework-connector');
const { contentBubbleTitles } = require('../models/contentbubbleTitle');
const AdaptiveCard = require('../resources/adaptiveCard.json');
const templateJson = require('../resources/QuestionTemplate.json');
const notificationCardJson = require('../resources/SendTargetNotificationCard.json');
const axios = require('axios');
var ACData = require("adaptivecards-templating");

class MeetingNotificationBot extends TeamsActivityHandler {
  constructor() {
    super();
    this.baseUrl = process.env.BaseUrl;
    this.AppId = process.env.MicrosoftAppId;

    // This method is invoked whenever there is any message activity in bot's chat.
    this.onMessage(async (context, next) => {
      var members = new Array();
      let meetingId = context.activity.channelData.meeting.id;

      if (context.activity.value == null) {
        TurnContext.removeRecipientMention(context.activity);

        if (context.activity.text.trim() == "SendTargetedNotification") {
          var meetingMembers = await TeamsInfo.getPagedMembers(context);
          let tenantId = context.activity.channelData.tenant.id;

          for (var member in meetingMembers) {
            let participantDetail = await TeamsInfo.getMeetingParticipant(context, meetingId, member.aadObjectId, tenantId);

            // Select only those members that present when meeting is started.
            if (participantDetail.meeting.inMeeting) {
              members.push({ id: participantDetail.user.id, name: participantDetail.user.name })
            }
          }

          // Send and adaptive card to user to select members for sending targeted notifications.
          await context.sendActivity({ attachments: [this.createMembersAdaptiveCard(members)] });
        }
        else if (context.activity.text.trim() == "SendContentBubble") {
          await context.sendActivity({ attachments: [this.createAdaptiveCard()] });
        }
        else {
          await context.sendActivity("Please type `SendTargetedNotification` or `SendContentBubble` to send In-meeting notifications.");
        }
      }
      else if (context.activity.value.Type == "SendTargetedMeetingNotification") {
        var adaptiveCardChoiceSet = context.activity.value.Choice;
        var selectedMembers = adaptiveCardChoiceSet.split(",");
        this.targetedNotification(context, meetingId, selectedMembers);
      }
      else {
        var json = JSON.stringify(context.activity.value);
        var out = JSON.parse(json);
        
        if (out.action == 'inputselector') {
          contentBubbleTitles.contentQuestion = out.myReview;
          await this.contentBubble(context);
          await context.sendActivity({ attachments: [this.createQuestionAdaptiveCard(out.myReview)] });
        } else {
          await context.sendActivity(context.activity.from.name + " : " + "**" + out.myReview + "**" + " for " + "'" + out.action + "'");
        }
      }

      await next();
    });
  };

  // Bot's submit handler for task module submit.
  async handleTeamsTaskModuleSubmit(context, taskModuleRequest) {
    var review = JSON.stringify(taskModuleRequest.data);
    var reply = JSON.parse(review);
    await context.sendActivity(context.activity.from.name + " : " + "**" + reply.myValue + "**" + " for " + "'" + reply.title + "'")
  }

  // Custom method for sending targeted meeting notifications.
  async targetedNotification(context, meetingId, selectedMembers) {
    var serviceUrl = context.activity.serviceUrl;
    const credentials = new MicrosoftAppCredentials(process.env.MicrosoftAppId, process.env.MicrosoftAppPassword);
    const botToken = await credentials.getToken();

    // Notification payload for meeting target notification API.
    let notificationInformation = {
      type: "targetedMeetingNotification",
      value: {
        recipients: selectedMembers,
        surfaces: [
          {
            surface: "meetingStage",
            contentType: "task",
            content: {
              value: {
                height: "300",
                width: "400",
                title: "Targeted meeting Notification",
                url: `${this.baseUrl}`
              }
            }
          }
        ]
      }
    }

    try {
      
      await TeamsInfo.sendMeetingNotification(context, notificationInformation, meetingId);
    } catch (exception) {
      console.log(exception);
    }
  }

  // Configure and send meeting content bubble.
  async contentBubble(context) {
    const replyActivity = MessageFactory.text("**Please provide your valuable feedback**");
    replyActivity.channelData = {
      onBehalfOf: [
        {
          itemId: 0,
          mentionType: 'person',
          mri: context.activity.from.id,
          displayname: context.activity.from.name
        }
      ],
      notification: {
        alertInMeeting: true,
        externalResourceUrl: 'https://teams.microsoft.com/l/bubble/' + this.AppId + '?url=' + this.baseUrl + '&height=270&width=300&title=ContentBubbleinTeams&completionBotId=' + this.AppId
      }
    };
    await context.sendActivity(replyActivity);
  }

  // Create adaptive card for sending initial agenda.
  createAdaptiveCard() {
    return CardFactory.adaptiveCard(AdaptiveCard);
  }

  // Create adaptive card for send list of agenda questions.
  createQuestionAdaptiveCard(myText) {
    var templatePayload = templateJson;
    var template = new ACData.Template(templatePayload);
    
    var cardPayload = template.expand({
      $root: {
        name: myText
      }
    });

    return CardFactory.adaptiveCard(cardPayload);
  }

  // Create adaptive card for send list of in-meeting participants.
  createMembersAdaptiveCard(members) {
    var templatePayload = notificationCardJson;
    var template = new ACData.Template(templatePayload);

    var cardPayload = template.expand({
      $root: {
        members: members
      }
    });

    return CardFactory.adaptiveCard(cardPayload);
  }
}

module.exports.MeetingNotificationBot = MeetingNotificationBot;
