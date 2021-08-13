// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler , MessageFactory } = require('botbuilder');

class BotActivityHandler extends TeamsActivityHandler  {
    constructor() {
        super();

        var sampleDescription = "With this sample your bot can receive user messages across standard channels in a team without being @mentioned";
        var options = "Press 1 to know about the permissions required,  Press 2 for documentation link"
        var permissionRequired = "This capability is enabled by specifying the ChannelMessage.Read.Group permission in the manifest of an RSC enabled Teams app";
        var docLink = "https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/channel-messages-with-rsc";
        
        this.onMessage(async (context, next) => {
            var replyText = context.activity.text;
            if(replyText == 1)
            {
              await context.sendActivity(MessageFactory.text(permissionRequired));
            }
            else if(replyText == 2){
              await context.sendActivity(MessageFactory.text(docLink));
            }
            else{
              await context.sendActivity(MessageFactory.text(sampleDescription));  
              await context.sendActivity(MessageFactory.text(options));
            }
            await next();
        });

        this.onMembersAdded(async (context, next) => {
          var welcomeText = "Hello and welcome! With this sample your bot can receive user messages across standard channels in a team without being @mentioned";
          await context.sendActivity(MessageFactory.text(welcomeText));
          await next();
      });
    }
}

module.exports.BotActivityHandler = BotActivityHandler;