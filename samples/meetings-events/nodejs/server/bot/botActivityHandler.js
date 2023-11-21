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

  //Invoked when a Meeting Started event activity is received from the connector.
  async onTeamsMeetingStart(context) {
    // Get the state properties from the turn context.
    const conversationData = await this.conversationDataAccessor.get(
      context, { startTime: '' });
    var meetingObject = context.activity.value;
    conversationData.startTime = meetingObject.StartTime;
    await context.sendActivity({ attachments: [CardFactory.adaptiveCard(adaptiveCards.adaptiveCardForMeetingStart(meetingObject))] });
    
    // Save any state changes. The load happened during the execution of the Dialog.
    await this.conversationState.saveChanges(context, false);
  };

  //Invoked when a Meeting Ended event activity is received from the connector.
  async onTeamsMeetingEnd(context) {
    // Get the state properties from the turn context.
    const conversationData = await this.conversationDataAccessor.get(
      context, { startTime: '' });
    var meetingObject = context.activity.value;
    var startTime = conversationData.startTime;
    var timeDuration = new Date(meetingObject.EndTime) - new Date(startTime);
    var minutes = Math.floor(timeDuration / 60000);
    var seconds = ((timeDuration % 60000) / 1000).toFixed(0);
    var meetingDurationText = minutes >= 1 ? minutes + "min " + seconds + "s": seconds + "s";
    await context.sendActivity({ attachments: [CardFactory.adaptiveCard(adaptiveCards.adaptiveCardForMeetingEnd(meetingObject, meetingDurationText))] });
  };

  //Invoked when a Meeting Participant Join event activity is received from the connector.
  async onTeamsMeetingParticipantsJoin(context) {
    await context.sendActivity({ 
        attachments: [CardFactory.adaptiveCard(adaptiveCards.adaptiveCardForMeetingParticipantEvents(context.activity.value.members[0].user.name, " has joined the meeting."))] 
    });
  };

  //Invoked when a Meeting Participant Leave event activity is received from the connector.
  async onTeamsMeetingParticipantsLeave(context) {
    await context.sendActivity({ 
      attachments: [CardFactory.adaptiveCard(adaptiveCards.adaptiveCardForMeetingParticipantEvents(context.activity.value.members[0].user.name, " left the meeting."))] 
    });
  };
}

module.exports.BotActivityHandler = BotActivityHandler;
