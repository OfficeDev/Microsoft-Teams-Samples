// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory } = require('@microsoft/agents-hosting');
const { TeamsActivityHandler } = require('@microsoft/agents-hosting-extensions-teams');
const ACData = require("adaptivecards-templating");
const helloWorldCard = require("./adaptiveCards/helloWorldCard.json");

// Helper function to remove bot mentions from a message
function removeRecipientMention(activity) {
    const text = activity.text || "";
    const recipientId = activity.recipient?.id;

    if (!recipientId) return text;
    
    // Remove <at>mention</at> tags for the bot
    return text.replace(/<at>.*?<\/at>/g, "").trim();
}

class ActionApp extends TeamsActivityHandler {
  constructor() {
    super();

    // Echo bot code
    this.onMessage(async (context, next) => {
      const text = removeRecipientMention(context.activity);
      if (text != undefined && text !== '') {
        const cleanText = text.trim();
        await context.sendActivity('You said : ' + cleanText);
      }
      await next();
    });
  }

  // Action.
  handleTeamsMessagingExtensionSubmitAction(context, action) {
    // The user has chosen to create a card by choosing the 'Create Card' context menu command.
    const template = new ACData.Template(helloWorldCard);
    const card = template.expand({
      $root: {
        title: action.data.title ?? "",
        subTitle: action.data.subTitle ?? "",
        text: action.data.text ?? "",
      },
    });
    const attachment = CardFactory.adaptiveCard(card);
    return {
      composeExtension: {
        type: "result",
        attachmentLayout: "list",
        attachments: [attachment],
      },
    };
  }
}

module.exports.ActionApp = ActionApp;
