// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, TurnContext } = require("botbuilder");
const path = require('path');

// Read botFilePath and botFileSecret from .env file.
require('dotenv').config({ path: path.resolve(__dirname, '../env/.env.local') }); // If deploying or provisioning the sample, please replace this with .env.dev

/**
 * TeamsBot class extends TeamsActivityHandler to handle Teams-specific activities.
 */
class TeamsBot extends TeamsActivityHandler {
  /**
   * Constructor for TeamsBot.
   * @param {Object} conversationReferences - Dictionary for storing ConversationReference objects.
   */
  constructor(conversationReferences) {
    super();
    this.conversationReferences = conversationReferences;

    this.onConversationUpdate(async (context, next) => {
      this.addConversationReference(context.activity);
      await next();
    });

    this.onMembersAdded(async (context, next) => {
      const membersAdded = context.activity.membersAdded;
      for (const member of membersAdded) {
        if (member.id !== context.activity.recipient.id) {
          const welcomeMessage = `Welcome to the Proactive Bot sample. Navigate to ${process.env.PROVISIONOUTPUT_BOTOUTPUT_SITEENDPOINT}/api/notify to proactively message everyone who has previously messaged this bot.`;
          await context.sendActivity(welcomeMessage);
        }
      }
      await next();
    });

    this.onMessage(async (context, next) => {
      this.addConversationReference(context.activity);
      await context.sendActivity(`You sent '${context.activity.text}'. Navigate to ${process.env.PROVISIONOUTPUT_BOTOUTPUT_SITEENDPOINT}/api/notify to proactively message everyone who has previously messaged this bot.`);
      await next();
    });
  }

  /**
   * Adds a conversation reference to the dictionary.
   * @param {Object} activity - The activity object from the context.
   */
  addConversationReference(activity) {
    const conversationReference = TurnContext.getConversationReference(activity);
    this.conversationReferences[conversationReference.conversation.id] = conversationReference;
  }
}

module.exports.TeamsBot = TeamsBot;