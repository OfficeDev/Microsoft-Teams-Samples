const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const path = require('path');
const dotenv = require('dotenv');

// Import people picker card templates
const PeoplePickerPersonalScope = require('./resources/peoplePickerPersonalScope.json');
const PeoplePickerChannelScope = require('./resources/peoplePickerChannelScope.json');

// Load environment variables
const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });

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

// Error handling for unhandled app errors
app.on("error", async (error) => {
  console.error("App error:", error);
});const getConversationState = (conversationId) => {
  let state = storage.get(conversationId);
  if (!state) {
    state = { count: 0 };
    storage.set(conversationId, state);
  }
  return state;
};

/**
 * Handle membersAdded event to send welcome message
 */
app.on("install.add", async (context) => {
  const activity = context.activity;

  // Check if membersAdded exists and is an array before iterating
  if (activity.membersAdded && Array.isArray(activity.membersAdded)) {
    // Iterate over all new members added to the conversation
    for (const member of activity.membersAdded) {
      if (member.id !== activity.recipient.id) {
        await context.send({
          type: 'message',
          text: 'Hello and welcome! With this sample you can see the functionality of people-picker in adaptive card. Send any message to see the people picker in action.'
        });
        
        // Send initial help information
        await sendHelpMessage(context);
      }
    }
  } else {
    // Fallback: send welcome message even if membersAdded is not available
    await context.send({
      type: 'message',
      text: 'Hello and welcome! With this sample you can see the functionality of people-picker in adaptive card. Send any message to see the people picker in action.'
    });
    
    // Send initial help information
    await sendHelpMessage(context);
  }
});

/**
 * Utility function to remove mention text from activity
 */
function removeMentionText(activity) {
  if (activity.entities && activity.entities.length > 0) {
    for (const entity of activity.entities) {
      if (entity.type === "mention" && entity.text) {
        activity.text = activity.text.replace(entity.text, "").trim();
      }
    }
  }
  return activity;
}

app.on("message", async (context) => {
  const activity = context.activity;
  
  // Remove mention text using both the Teams AI SDK method and custom method
  let cleanedActivity = removeMentionText(activity);
  const text = stripMentionsText(cleanedActivity);

  // Handle adaptive card submissions
  if (activity.value && (activity.value.taskTitle || activity.value.submitdynamic)) {
    const submission = activity.value;
    let assignedUsers = submission.userId;
    
    // Handle both single and multiple user assignments
    if (Array.isArray(assignedUsers)) {
      assignedUsers = assignedUsers.join(', ');
    } else if (!assignedUsers) {
      assignedUsers = 'No user selected';
    }

    const responseText = `**Task Assignment Complete**\n\n` +
      `**Task Title:** ${submission.taskTitle || 'Not specified'}\n` +
      `**Task Description:** ${submission.taskDescription || 'Not specified'}\n` +
      `**Assigned To:** ${assignedUsers}`;

    await context.send({
      type: 'message',
      text: responseText
    });
    return;
  }

  // Handle debug commands
  if (text === "/reset") {
    storage.delete(activity.conversation.id);
    await context.send("Ok I've deleted the current conversation state.");
    return;
  }

  if (text === "/count") {
    const state = getConversationState(activity.conversation.id);
    await context.send(`The count is ${state.count}`);
    return;
  }

  if (text === "/diag") {
    await context.send(`\`\`\`json\n${JSON.stringify(activity, null, 2)}\n\`\`\``);
    return;
  }

  if (text === "/state") {
    const state = getConversationState(activity.conversation.id);
    await context.send(`\`\`\`json\n${JSON.stringify(state, null, 2)}\n\`\`\``);
    return;
  }

  if (text === "/runtime") {
    const runtime = {
      nodeversion: process.version,
      sdkversion: "2.0.0", // Teams AI v2
      conversationType: activity.conversation.conversationType
    };
    await context.send(`\`\`\`json\n${JSON.stringify(runtime, null, 2)}\n\`\`\``);
    return;
  }

  // Send help message
  if (text.toLowerCase() === "help" || text === "/help") {
    await sendHelpMessage(context);
    return;
  }

  // Send people picker card based on conversation type
  if (text && text.trim() !== "") {
    await sendPeoplePickerCard(context);
    
    // Update conversation state
    const state = getConversationState(activity.conversation.id);
    state.count++;
    return;
  }

  // Default behavior for empty or whitespace-only messages
  await context.send("Please send a message to see the people picker functionality. Type `help` for available commands.");
});

/**
 * Send help message with available commands
 * @param {Object} context The turn context.
 */
async function sendHelpMessage(context) {
  const helpText = `**People Picker Bot Commands:**\n\n` +
    `• Send any message to see the people picker card\n` +
    `• \`/reset\` - Clear conversation state\n` +
    `• \`/count\` - Show message count\n` +
    `• \`/state\` - Show conversation state\n` +
    `• \`/runtime\` - Show runtime info\n` +
    `• \`/diag\` - Show diagnostic info\n` +
    `• \`help\` - Show this help message\n\n` +
    `**Note:** The people picker behavior changes based on conversation scope:\n` +
    `• Personal chat: Single user selection\n` +
    `• Channel/Group: Multiple user selection`;
  
  await context.send({
    type: 'message',
    text: helpText
  });
}

/**
 * Send people picker card based on conversation scope
 * @param {Object} context The turn context.
 */
async function sendPeoplePickerCard(context) {
  const activity = context.activity;
  const isPersonalScope = activity.conversation.conversationType === "personal";
  const cardTemplate = isPersonalScope ? PeoplePickerPersonalScope : PeoplePickerChannelScope;

  try {
    // Add some context about what type of card is being sent
    const scopeInfo = isPersonalScope ? 
      "Here's a people picker card for personal scope (single selection):" :
      "Here's a people picker card for channel/group scope (multiple selection):";

    await context.send({
      type: 'message',
      text: scopeInfo
    });

    await context.send({
      type: 'message',
      attachments: [{
        contentType: 'application/vnd.microsoft.card.adaptive',
        content: cardTemplate
      }]
    });
  } catch (error) {
    console.error("Error sending people picker card:", error);
    await context.send(`Error sending people picker card: ${error.message}\n\nTry typing \`help\` for available commands.`);
  }
}

module.exports = app;
