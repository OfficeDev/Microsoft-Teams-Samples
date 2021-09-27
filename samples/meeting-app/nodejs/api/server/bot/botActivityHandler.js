// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler , MessageFactory } = require('botbuilder');

class BotActivityHandler extends TeamsActivityHandler  {
    constructor() {
        super();

        // Called when the bot is added to a team.
        this.onMembersAdded(async (context, next) => {
          var welcomeText = "Hello and welcome!";
          await context.sendActivity(MessageFactory.text(welcomeText));
          await next();
      });

         // Activity called when there's a message in channel
         this.onMessage(async (context, next) => {
          var replyText = context.activity.text;
          await context.sendActivity(MessageFactory.text(replyText));
          await next();
      });
    }
}

module.exports.BotActivityHandler = BotActivityHandler;
