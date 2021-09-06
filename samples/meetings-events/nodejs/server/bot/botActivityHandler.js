// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory } = require('botbuilder');
const adaptiveCards = require('../models/adaptiveCard');

// The accessor name for the conversation data property accessor.
const MEETING_DATA_PROPERTY = 'meetingData';

class BotActivityHandler extends TeamsActivityHandler {
  constructor(conversationState) {
    super();
    // Create the state property accessor for the conversation data.
    this.conversationDataAccessor = conversationState.createProperty(MEETING_DATA_PROPERTY);

    // The state management object for the conversation state.
    this.conversationState = conversationState;
  }

  async onTurnActivity(context) {
    // Get the state properties from the turn context.
    const conversationData = await this.conversationDataAccessor.get(
      context, { startTime: '' });
    if (context.activity.type == 'event' && context.activity.name == "application/vnd.microsoft.meetingStart") {
      var meetingObject = context.activity.value;
      conversationData.startTime = meetingObject.StartTime;
      await context.sendActivity({ attachments: [CardFactory.adaptiveCard(adaptiveCards.adaptiveCardForMeetingStart(meetingObject))] });
      
      // Save any state changes. The load happened during the execution of the Dialog.
      await this.conversationState.saveChanges(context, false);
    }

    if (context.activity.type == 'event' && context.activity.name == "application/vnd.microsoft.meetingEnd") {
      var meetingObject = context.activity.value;
      var startTime = conversationData.startTime;
      var timeDuration = new Date(meetingObject.EndTime) - new Date(startTime);
      var minutes = Math.floor(timeDuration / 60000);
      var seconds = ((timeDuration % 60000) / 1000).toFixed(0);
      var meetingDurationText = minutes >= 1 ? minutes + "min " + seconds + "s": seconds + "s";
      await context.sendActivity({ attachments: [CardFactory.adaptiveCard(adaptiveCards.adaptiveCardForMeetingEnd(meetingObject, meetingDurationText))] });
    }
  };
}

module.exports.BotActivityHandler = BotActivityHandler;
