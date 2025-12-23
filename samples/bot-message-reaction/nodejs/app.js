const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const { ActivityLog } = require("./activityLog");

// Create storage for conversation history
const storage = new LocalStorage();

// Create activity log for tracking sent messages
const activityLog = new ActivityLog(storage);

const createTokenFactory = () => {
  return async (scope, tenantId) => {
    const managedIdentityCredential = new ManagedIdentityCredential({
      clientId: process.env.CLIENT_ID,
    });
    const scopes = Array.isArray(scope) ? scope : [scope];
    const tokenResponse = await managedIdentityCredential.getToken(scopes, {
      tenantId: tenantId,
    });

    return tokenResponse.token;
  };
};

// Configure authentication using TokenCredentials
const tokenCredentials = {
  clientId: process.env.CLIENT_ID || "",
  token: createTokenFactory(),
};

const credentialOptions =
  config.MicrosoftAppType === "UserAssignedMsi" ? { ...tokenCredentials } : undefined;

// Create the app with storage
const app = new App({
  ...credentialOptions,
  storage,
});

const getConversationState = (conversationId) => {
  let state = storage.get(conversationId);
  if (!state) {
    state = { count: 0 };
    storage.set(conversationId, state);
  }
  return state;
};

app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity);

  // Simple echo behavior like the original sample
  const replyText = `Echo: MessageReactionBot ${text}`;
  
  // Send message and log the activity ID for reaction tracking
  const response = await context.send(replyText);
  
  // Log the sent message with its activity ID
  if (response && response.id) {
    await activityLog.append(response.id, {
      text: replyText,
      type: 'message',
      conversation: activity.conversation
    });
  }
});

// Handle message reactions added
app.on("messageReaction", async (context) => {
  const activity = context.activity;
  
  if (activity.reactionsAdded && activity.reactionsAdded.length > 0) {
    for (const reaction of activity.reactionsAdded) {
      // Find the original message that was reacted to
      const originalActivity = await activityLog.find(
        activity.replyToId, 
        activity.conversation.id
      );
      
      if (originalActivity) {
        await context.send(`You added '${reaction.type}' regarding '${originalActivity.text}'`);
      } else {
        await context.send(`Activity ${activity.replyToId} not found in the log.`);
      }
    }
  }
  
  if (activity.reactionsRemoved && activity.reactionsRemoved.length > 0) {
    for (const reaction of activity.reactionsRemoved) {
      // Find the original message that had the reaction removed
      const originalActivity = await activityLog.find(
        activity.replyToId, 
        activity.conversation.id
      );
      
      if (originalActivity) {
        await context.send(`You removed '${reaction.type}' regarding '${originalActivity.text}'`);
      } else {
        await context.send(`Activity ${activity.replyToId} not found in the log.`);
      }
    }
  }
});

module.exports = app;
