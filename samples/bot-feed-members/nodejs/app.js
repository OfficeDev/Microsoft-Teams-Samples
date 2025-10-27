const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const path = require('path');
const dotenv = require('dotenv');

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

const getConversationState = (conversationId) => {
  let state = storage.get(conversationId);
  if (!state) {
    state = { count: 0 };
    storage.set(conversationId, state);
  }
  return state;
};

/**
 * Handle bot installation - when bot is added to a conversation
 */
app.on("install.add", async (context) => {
  const activity = context.activity;
  
  console.log("Bot installed/added to conversation:", activity.conversation.id);
  
  // Send a general welcome message when the bot is installed
  await context.send({
    type: 'message',
    text: 'Hello! I\'m your Member Management Bot. I can help you manage team members in this conversation.'
  });
  
  // Send the member management options
  await sendMemberManagementCard(context);
});

/**
 * Handle when members are added to the conversation
 */
app.on("conversationUpdate", async (context) => {
  const activity = context.activity;

  // Check if this is a members added event
  if (!activity.membersAdded || !Array.isArray(activity.membersAdded)) {
    console.log("No membersAdded in conversationUpdate, or not an array:", activity.membersAdded);
    return;
  }

  // Additional safety check for empty array
  if (activity.membersAdded.length === 0) {
    console.log("No members to process in membersAdded array");
    return;
  }

  console.log(`Processing ${activity.membersAdded.length} new members`);

  // Iterate over all new members added to the conversation
  for (const member of activity.membersAdded) {
    try {
      // Check if member object is valid
      if (!member || !member.id) {
        console.warn("Invalid member object:", member);
        continue;
      }

      // Skip if this is the bot itself being added
      if (member.id === activity.recipient.id) {
        console.log("Bot was added to the conversation, skipping welcome message");
        continue;
      }

      console.log(`Welcoming new member: ${member.name || member.id}`);

      await context.send({
        type: 'message',
        text: 'Welcome to the team! I can help you manage team members. Use "list" to see all members or click the button below.'
      });
      
      try {
        // Get detailed information about the new member using Teams AI SDK
        const conversationId = activity.conversation.id;
        const memberDetails = await context.api.conversations.members(conversationId).getById(member.id);
        await sendMemberWelcomeCard(context, memberDetails);
      } catch (error) {
        console.error("Error getting member details:", error);
        await context.send("I couldn't get your detailed information, but welcome to the team!");
      }
      
      // Send the member management options
      await sendMemberManagementCard(context);
    } catch (error) {
      console.error("Error processing member:", member, "Error:", error);
    }
  }
});

/**
 * Handle message events
 */
app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity);

  // Create an array with the valid commands
  const memberCommands = ['list', 'List', 'members', 'Members'];
  const helpCommands = ['help', 'Help', '?'];

  // Handle different command types
  if (memberCommands.some(command => text.includes(command))) {
    try {
      // Get conversation members using Teams AI SDK
      const conversationId = activity.conversation.id;
      const members = await context.api.conversations.members(conversationId).get();
      await listMembersAsync(context, members);
    } catch (error) {
      console.error("Error getting members:", error);
      await context.send("Sorry, I couldn't retrieve the member list. Please try again.");
    }
  } else if (helpCommands.some(command => text.includes(command))) {
    await sendHelpCard(context);
  } else {
    // Default behavior - send welcome message
    await sendWelcomeCard(context);
  }

  // After the bot has responded, send the suggested actions
  await sendMemberManagementCard(context);
});

/**
 * Lists all members in the current Teams conversation using an Adaptive Card
 * @param {Object} context - The turn context
 * @param {Array} members - The list of conversation members
 */
async function listMembersAsync(context, members) {
  // Handle both array and object with members property
  const memberArray = Array.isArray(members) ? members : (members.members || []);
  
  const adaptiveCardJson = {
    type: 'AdaptiveCard',
    body: [
      {
        type: 'TextBlock',
        text: 'Team Members',
        weight: 'Bolder',
        size: 'Large'
      },
      {
        type: 'TextBlock',
        text: `Total members: ${memberArray.length}`,
        weight: 'Lighter',
        size: 'Small',
        spacing: 'None'
      },
      {
        type: 'Container',
        items: memberArray.map((member, index) => ({
          type: 'FactSet',
          facts: [
            { 
              title: `${index + 1}.`, 
              value: member.name || member.displayName || 'Unknown'
            }
          ],
          spacing: 'Small'
        })),
        spacing: 'Medium'
      }
    ],
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    version: '1.4'
  };

  try {
    await context.send({
      type: 'message',
      attachments: [{
        contentType: 'application/vnd.microsoft.card.adaptive',
        content: adaptiveCardJson
      }]
    });
  } catch (error) {
    console.error("Error sending member list:", error);
    await context.send("Error sending member list: " + error.message);
  }
}

/**
 * Sends a welcome card with an option to list all members
 * @param {Object} context - The turn context
 */
async function sendWelcomeCard(context) {
  await context.send({
    type: 'message',
    text: 'Hello! I can help you manage team members.'
  });
}

/**
 * Send member management options to the user.
 * @param {Object} context The turn context.
 */
async function sendMemberManagementCard(context) {
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.hero',
      content: {
        title: 'Member Management',
        text: 'What would you like to do?',
        buttons: [
          {
            type: 'imBack',
            title: 'List Members',
            value: 'list'
          },
          {
            type: 'imBack', 
            title: 'Help',
            value: 'help'
          }
        ]
      }
    }]
  });
}

/**
 * Send help information to the user.
 * @param {Object} context The turn context.
 */
async function sendHelpCard(context) {
  const helpCard = {
    type: "AdaptiveCard",
    body: [
      {
        type: "TextBlock",
        text: "Member Management Bot Help",
        weight: "Bolder",
        size: "Large"
      },
      {
        type: "TextBlock",
        text: "I can help you manage team members in your Teams conversation.",
        wrap: true,
        spacing: "Medium"
      },
      {
        type: "TextBlock",
        text: "Available Commands:",
        weight: "Bolder",
        spacing: "Medium"
      },
      {
        type: "FactSet",
        facts: [
          { title: "list or members", value: "Show all members in the conversation" },
          { title: "help or ?", value: "Show this help message" },
          { title: "Any other text", value: "Show welcome message with options" }
        ]
      },
      {
        type: "TextBlock",
        text: "Features:",
        weight: "Bolder",
        spacing: "Medium"
      },
      {
        type: "Container",
        items: [
          {
            type: "TextBlock",
            text: "• Automatic welcome for new members",
            wrap: true
          },
          {
            type: "TextBlock",
            text: "• Interactive member listing with Adaptive Cards",
            wrap: true
          },
          {
            type: "TextBlock",
            text: "• Member details display",
            wrap: true
          }
        ]
      }
    ],
    $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
    version: "1.4"
  };

  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.adaptive',
      content: helpCard
    }]
  });
}

/**
 * Sends a welcome message with member details when someone joins
 * @param {Object} context - The turn context
 * @param {Object} member - The member details
 */
async function sendMemberWelcomeCard(context, member) {
  const memberCard = {
    type: "AdaptiveCard",
    body: [
      {
        type: "TextBlock",
        text: `Welcome to the team, ${member.name}!`,
        weight: "Bolder",
        size: "Large"
      },
      {
        type: "TextBlock",
        text: "Here are your details:",
        weight: "Bolder",
        size: "Medium",
        spacing: "Medium"
      },
      {
        type: "FactSet",
        facts: [
          { title: "Name:", value: member.name || member.displayName || "N/A" },
          { title: "Email:", value: member.email || "N/A" },
          { title: "Given Name:", value: member.givenName || "N/A" },
          { title: "Surname:", value: member.surname || "N/A" },
          { title: "User Principal Name:", value: member.userPrincipalName || "N/A" },
          { title: "ID:", value: member.id || "N/A" }
        ].filter(fact => fact.value !== "N/A"), // Only show available information
        spacing: "Medium"
      }
    ],
    $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
    version: "1.4"
  };

  try {
    await context.send({
      type: 'message',
      attachments: [{
        contentType: 'application/vnd.microsoft.card.adaptive',
        content: memberCard
      }]
    });
  } catch (error) {
    console.error("Error sending welcome card:", error);
    await context.send("Error sending welcome card: " + error.message);
  }
}

module.exports = app;
