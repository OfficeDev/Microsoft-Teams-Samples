const { TeamsActivityHandler, TurnContext } = require("botbuilder");

require('dotenv').config({ path: './env/.env.local' });

class TeamsBot extends TeamsActivityHandler {
  constructor(conversationReferences) {
    super();
    
    this.conversationReferences = conversationReferences;

    this.onConversationUpdate(async (context, next) => {
      this.addConversationReference(context.activity);

      await next();
    });

    this.onMessage(async (context, next) => {
      this.addConversationReference(context.activity);
      await context.sendActivity(`You sent '${context.activity.text}'. Navigate to ${process.env.BOT_ENDPOINT}/api/notify to proactively message everyone who has previously messaged this bot.`);
      await next();
    });

    this.onMembersAdded(async (context, next) => {
      const membersAdded = context.activity.membersAdded;
      for (let cnt = 0; cnt < membersAdded.length; cnt++) {
        if (membersAdded[cnt].id !== context.activity.recipient.id) {
          const welcomeMessage = `Welcome to the Proactive Bot sample.  Navigate to ${process.env.BOT_ENDPOINT}/api/notify to proactively message everyone who has previously messaged this bot.`;
          await context.sendActivity(welcomeMessage);
        }
      }
      await next();
    });
  }

  addConversationReference(activity) {
    const conversationReference = TurnContext.getConversationReference(activity);
    this.conversationReferences[conversationReference.conversation.id] = conversationReference;
  }
}

module.exports.TeamsBot = TeamsBot;
