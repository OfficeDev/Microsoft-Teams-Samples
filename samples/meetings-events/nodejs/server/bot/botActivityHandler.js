// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler , CardFactory } = require('botbuilder');
const adaptiveCards = require('../models/adaptiveCard');

class BotActivityHandler extends TeamsActivityHandler  {
    constructor() {
        super();
    }

    async onTurnActivity(context){
      console.log(context);
      if(context.activity.type == 'event' && context.activity.name == "application/vnd.microsoft.meetingStart")
      {
        var meetingObject = context.activity.value;
        await context.sendActivity({ attachments: [CardFactory.adaptiveCard(adaptiveCards.adaptiveCardForMeetingStart(meetingObject))] });
      }

      if(context.activity.type == 'event' && context.activity.name == "application/vnd.microsoft.meetingEnd")
      {
        var meetingObject = context.activity.value;
        await context.sendActivity({ attachments: [CardFactory.adaptiveCard(adaptiveCards.adaptiveCardForMeetingEnd(meetingObject))] });
      }
    };
}

module.exports.BotActivityHandler = BotActivityHandler;
