const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");

// Create storage for conversation history
const storage = new LocalStorage();

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

// Handle all messages (including RSC channel messages)
app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity);
  
  // Log incoming messages for RSC demonstration
  console.log(`� Message Received: "${text}" from ${activity.from?.name} in ${activity.conversation?.conversationType || 'unknown context'}`);
  
  // Check if this is a channel message (RSC enabled)
  const isChannelMessage = activity.conversation?.conversationType === 'channel';
  const isGroupChat = activity.conversation?.conversationType === 'groupChat';

  // Handle commands exactly like the original RSC sample
  if (text === "1") {
    const permissionRequired = "This capability is enabled by specifying the ChannelMessage.Read.Group permission in the manifest of an RSC enabled Teams app";
    await context.send(permissionRequired);
    return;
  }

  if (text === "2") {
    const docLink = "https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/channel-messages-with-rsc";
    await context.send(docLink);
    return;
  }



  // Default response - exactly matches original sample behavior
  const sampleDescription = "With this sample your bot can receive user messages across standard channels in a team without being @mentioned";
  const option = "Type 1 to know about the permissions required,  Type 2 for documentation link";

  const state = getConversationState(activity.conversation.id);
  state.count = (state.count || 0) + 1;
  
  await context.send(sampleDescription);
  await context.send(option);
});

// Handle bot installation - matches original sample exactly
app.on("membersAdded", async (context) => {
  const activity = context.activity;
  
  // Check if the bot was added
  if (activity.membersAdded?.some(member => member.id === activity.recipient.id)) {
    const welcomeText = "Hello and welcome! With this sample your bot can receive user messages across standard channels in a team without being @mentioned";
    await context.send(welcomeText);
    
    console.log(`🎉 RSC Bot installed - Context: ${activity.conversation?.conversationType}, Team: ${activity.channelData?.team?.name || 'N/A'}`);
  }
});

// Handle team renamed event
app.on("channelRenamed", async (context) => {
  const activity = context.activity;
  console.log(`📝 Channel renamed: ${activity.channelData?.channel?.name} (${activity.channelData?.channel?.id})`);
});

// Handle team members added
app.on("teamMemberAdded", async (context) => {
  const activity = context.activity;
  console.log(`👥 New team member added to: ${activity.channelData?.team?.name}`);
});

module.exports = app;
