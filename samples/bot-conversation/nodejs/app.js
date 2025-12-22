/**
 * Teams Conversation Bot - Enhanced Implementation
 * 
 * This implementation combines functionality from:
 * - nodejs sample (Bot Framework v4) - for conversation features
 * - bot-all-cards-teams-ai sample - for Teams AI SDK v2 patterns
 * 
 * Teams AI SDK patterns referenced from bot-all-cards-teams-ai:
 * - Event handling with app.on()
 * - Card attachment structure
 * - Button types (imBack, messageBack)
 * - Context.send() message pattern
 * - Member addition handling
 */

const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const path = require('path');
const dotenv = require('dotenv');
const ACData = require('adaptivecards-templating');

// Import card templates
const AdaptiveCardTemplate = require('./resources/UserMentionCardTemplate.json');
const ImmersiveReaderCardTemplate = require('./resources/ImmersiveReaderCard.json');

// Load environment variables
const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });

// Create storage for conversation history and conversation state
const storage = new LocalStorage();

// Global variables for read receipt tracking
let counter = 0;
let users = [];
let teamMemberDetails = [];

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
    state = { 
      count: 0, 
      welcomeCardData: { count: 0 },
      readReceiptTracking: {
        counter: 0,
        users: [],
        teamMemberDetails: []
      }
    };
    storage.set(conversationId, state);
  }
  return state;
};

// Helper function to get read receipt state for a conversation
const getReadReceiptState = (conversationId) => {
  const state = getConversationState(conversationId);
  if (!state.readReceiptTracking) {
    state.readReceiptTracking = {
      counter: 0,
      users: [],
      teamMemberDetails: []
    };
    storage.set(conversationId, state);
  }
  return state.readReceiptTracking;
};

/**
 * Handle membersAdded event - Send welcome message when new members join
 * Following bot-all-cards-teams-ai pattern
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
          text: 'Welcome to Teams Conversation Bot! This bot demonstrates various Teams conversation events and features. Please select from the options below to explore different capabilities.'
        });
        
        // Send the welcome card with options
        await sendWelcomeCard(context);
      }
    }
  }
});

/**
 * Handle reactions added to messages
 */
app.on("reactionsAdded", async (context) => {
  const activity = context.activity;
  
  for (const reaction of activity.reactionsAdded || []) {
    const newReaction = `You reacted with '${reaction.type}' to the following message: '${activity.replyToId}'`;
    await context.send(newReaction);
  }
});

/**
 * Handle reactions removed from messages
 */
app.on("reactionsRemoved", async (context) => {
  const activity = context.activity;
  
  for (const reaction of activity.reactionsRemoved || []) {
    const newReaction = `You removed the reaction '${reaction.type}' from the message: '${activity.replyToId}'`;
    await context.send(newReaction);
  }
});

/**
 * Handle message edit events
 */
app.on("messageEdit", async (context) => {
  const editedMessage = context.activity.text;
  await context.send(`The edited message is ${editedMessage}"`);
});

/**
 * Handle message undelete events
 */
app.on("messageUndelete", async (context) => {
  const undeletedMessage = context.activity.text;
  await context.send(`Previously the message was deleted. After undeleting, the message is now: "${undeletedMessage}"`);
});

/**
 * Handle message soft delete events
 */
app.on("messageDelete", async (context) => {
  await context.send("Message is soft deleted");
});

/**
 * Handle channel created events
 */
app.on("channelCreated", async (context) => {
  const channelName = context.activity.channelData?.channel?.name || "Unknown Channel";
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.hero',
      content: {
        title: 'Channel Created',
        text: `${channelName} is new the Channel created`
      }
    }]
  });
});

/**
 * Handle channel renamed events
 */
app.on("channelRenamed", async (context) => {
  const channelName = context.activity.channelData?.channel?.name || "Unknown Channel";
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.hero',
      content: {
        title: 'Channel Renamed',
        text: `${channelName} is the new Channel name`
      }
    }]
  });
});

/**
 * Handle channel deleted events
 */
app.on("channelDeleted", async (context) => {
  const channelName = context.activity.channelData?.channel?.name || "Unknown Channel";
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.hero',
      content: {
        title: 'Channel Deleted',
        text: `${channelName} is deleted`
      }
    }]
  });
});

/**
 * Handle channel restored events
 */
app.on("channelRestored", async (context) => {
  const channelName = context.activity.channelData?.channel?.name || "Unknown Channel";
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.hero',
      content: {
        title: 'Channel Restored',
        text: `${channelName} is the Channel restored`
      }
    }]
  });
});

/**
 * Handle team renamed events
 */
app.on("teamRenamed", async (context) => {
  const teamName = context.activity.channelData?.team?.name || "Unknown Team";
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.hero',
      content: {
        title: 'Team Renamed',
        text: `${teamName} is the new Team name`
      }
    }]
  });
});

/**
 * Handle read receipt events (when users read messages sent by the bot)
 */
app.on("readReceipt", async (context) => {
  const readReceiptInfo = context.activity.readReceiptInfo;
  const member = context.activity.from;
  const readReceiptState = getReadReceiptState(context.activity.conversation.id);
  
  let memberDetails = readReceiptState.teamMemberDetails.find(m => m.aadObjectId === member.aadObjectId);
  if (memberDetails && readReceiptInfo && readReceiptInfo.isMessageRead(memberDetails.messageId)) {
    readReceiptState.users.push(memberDetails.name);
    readReceiptState.counter++;
    readReceiptState.teamMemberDetails = readReceiptState.teamMemberDetails.filter(m => m.aadObjectId !== member.aadObjectId);
    
    // Persist the updated state
    const state = getConversationState(context.activity.conversation.id);
    storage.set(context.activity.conversation.id, state);
    
    // Also update global variables for backward compatibility
    users.push(memberDetails.name);
    counter++;
    teamMemberDetails = teamMemberDetails.filter(m => m.aadObjectId !== member.aadObjectId);
  }
});

/**
 * Handle invoke activities (like adaptive card actions)
 */
app.on("invoke", async (context) => {
  try {
    const activity = context.activity;
    
    switch (activity.name) {
      case "message/submitAction":
        const reaction = activity.value.actionValue?.reaction || "No reaction";
        const feedback = activity.value.actionValue?.feedback ? 
          JSON.parse(activity.value.actionValue.feedback).feedbackText : "No feedback";
        
        await context.send(`Provided reaction: ${reaction}<br> Feedback: ${feedback}`);
        break;
      
      default:
        await context.send(`Unknown invoke activity handled as default - ${activity.name}`);
        break;
    }
  } catch (err) {
    console.log(`Error in invoke activity: ${err}`);
    await context.send(`Invoke activity received - ${context.activity.name}`);
  }
});

/**
 * Handle message events
 */
app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity).trim().toLowerCase();

  // Handle existing debug commands
  if (text === "/reset") {
    storage.delete(activity.conversation.id);
    await resetReadUserCount(context);
    await context.send("Ok I've deleted the current conversation state.");
    return;
  }

  if (text === "/count") {
    const state = getConversationState(activity.conversation.id);
    await context.send(`The count is ${state.count}`);
    return;
  }

  if (text === "/diag") {
    await context.send(JSON.stringify(activity));
    return;
  }

  if (text === "/state") {
    const state = getConversationState(activity.conversation.id);
    await context.send(JSON.stringify(state));
    return;
  }

  if (text === "/runtime") {
    const runtime = {
      nodeversion: process.version,
      sdkversion: "2.0.0", // Teams AI v2
    };
    await context.send(JSON.stringify(runtime));
    return;
  }

  // Define valid conversation commands - following bot-all-cards-teams-ai pattern
  const conversationCommands = [
    'Show Welcome', 'mention me', 'mention', 'who', 'message', 'aadid', 
    'immersivereader', 'check', 'reset', 'label', 'sensitivity', 
    'feedback', 'citation', 'aitext', 'update', 'delete'
  ];

  // Handle Teams conversation features
  if (conversationCommands.some(cmd => text.toLowerCase().includes(cmd.toLowerCase()))) {
    if (text.includes('show welcome') || text === 'show welcome') {
      await sendWelcomeCard(context);
    } else if (text.includes('mention me')) {
      await mentionAdaptiveCardActivity(context);
    } else if (text.includes('mention')) {
      await mentionActivity(context);
    } else if (text.includes('update')) {
      await updateCard(context);
    } else if (text.includes('delete')) {
      await deleteCardActivity(context);
    } else if (text.includes('aadid')) {
      await messageAllMembers(context, true);
    } else if (text.includes('message')) {
      await messageAllMembers(context, false);
    } else if (text.includes('who')) {
      await getSingleMember(context);
    } else if (text.includes('immersivereader')) {
      await getImmersiveReaderCard(context);
    } else if (text.includes('check')) {
      await checkReadUserCount(context);
    } else if (text.includes('reset')) {
      await resetReadUserCount(context);
    } else if (text.includes('label')) {
      await addAILabel(context);
    } else if (text.includes('sensitivity')) {
      await addSensitivityLabel(context);
    } else if (text.includes('feedback')) {
      await addFeedbackButtons(context);
    } else if (text.includes('citation')) {
      await addCitations(context);
    } else if (text.includes('aitext')) {
      await sendAIMessage(context);
    }

    // Send confirmation message following bot-all-cards-teams-ai pattern
    await context.send(`You selected: <b>${text}</b>`);
    
    // Show welcome card again for continued interaction
    await sendWelcomeCard(context);
  } else {
    // Default echo behavior with count
    const state = getConversationState(activity.conversation.id);
    state.count++;
    await context.send(`[${state.count}] you said: ${text}`);
  }
});

/**
 * Send a welcome card with action buttons
 * Following bot-all-cards-teams-ai pattern for consistency
 */
async function sendWelcomeCard(context) {
  const state = getConversationState(context.activity.conversation.id);
  
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.hero',
      content: {
        title: 'Teams Conversation Bot - Welcome!',
        text: 'This bot demonstrates Teams conversation events and features. Select an option below:',
        buttons: [
          {
            type: 'imBack',
            title: 'Show Welcome',
            value: 'Show Welcome'
          },
          {
            type: 'imBack',
            title: 'Mention Me',
            value: 'mention me'
          },
          {
            type: 'imBack',
            title: 'Who Am I?',
            value: 'who'
          },
          {
            type: 'imBack',
            title: 'Message All Members',
            value: 'message'
          },
          {
            type: 'imBack',
            title: 'Immersive Reader',
            value: 'immersivereader'
          },
          {
            type: 'imBack',
            title: 'AI Label Example',
            value: 'label'
          },
          {
            type: 'imBack',
            title: 'Citations Example',
            value: 'citation'
          },
          {
            type: 'imBack',
            title: 'Feedback Buttons',
            value: 'feedback'
          },
          {
            type: 'imBack',
            title: 'Sensitivity Label',
            value: 'sensitivity'
          },
          {
            type: 'imBack',
            title: 'Complete AI Message',
            value: 'aitext'
          }
        ]
      }
    }]
  });
}

/**
 * Update a card with incremented counter
 */
async function updateCard(context) {
  const state = getConversationState(context.activity.conversation.id);
  state.welcomeCardData.count += 1;
  
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.hero',
      content: {
        title: 'Updated card',
        text: `Update count: ${state.welcomeCardData.count}`,
        buttons: [
          {
            type: 'messageBack',
            title: 'Update Card',
            value: { count: state.welcomeCardData.count },
            text: 'UpdateCardAction'
          }
        ]
      }
    }]
  });
}

/**
 * Get information about a single member
 */
async function getSingleMember(context) {
  try {
    // Since we're using Teams AI SDK, we'll get member info from the activity
    const member = context.activity.from;
    await context.send(`You are: ${member.name || member.id}`);
  } catch (e) {
    await context.send('Member not found.');
  }
}

/**
 * Send an adaptive card with user mention
 */
async function mentionAdaptiveCardActivity(context) {
  try {
    const member = context.activity.from;
    
    const template = new ACData.Template(AdaptiveCardTemplate);
    const memberData = {
      userName: member.name || 'User',
      userUPN: member.userPrincipalName || member.id,
      userAAD: member.aadObjectId || member.id
    };

    const adaptiveCard = template.expand({
      $root: memberData
    });

    await context.send({
      type: 'message',
      attachments: [{
        contentType: 'application/vnd.microsoft.card.adaptive',
        content: adaptiveCard
      }]
    });
  } catch (e) {
    await context.send('Error creating mention card: ' + e.message);
  }
}

/**
 * Send a simple mention message
 */
async function mentionActivity(context) {
  const member = context.activity.from;
  const mentionText = `<at>${member.name || member.id}</at>`;
  
  await context.send({
    type: 'message',
    text: `Hi ${mentionText}`,
    entities: [{
      type: 'mention',
      text: mentionText,
      mentioned: {
        id: member.id,
        name: member.name || member.id
      }
    }]
  });
}

/**
 * Send an immersive reader card
 */
async function getImmersiveReaderCard(context) {
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.adaptive',
      content: ImmersiveReaderCardTemplate
    }]
  });
}

/**
 * Delete a card (Note: Teams AI SDK may have different implementation)
 */
async function deleteCardActivity(context) {
  try {
    // In Teams AI SDK, we'll send a message indicating deletion
    await context.send("Card deletion requested. (Note: Implementation may vary with Teams AI SDK)");
  } catch (e) {
    await context.send("Error deleting card: " + e.message);
  }
}

/**
 * Message all members in the conversation
 */
async function messageAllMembers(context, useAadId = false) {
  try {
    await resetReadUserCount(context);
    
    // Since we're using Teams AI SDK, member enumeration may work differently
    // For now, we'll provide a placeholder implementation
    await context.send("Messaging all members functionality is being implemented for Teams AI SDK.");
    
    // In a full implementation, you would:
    // 1. Get the list of conversation members
    // 2. Create individual conversations with each member
    // 3. Send personalized messages
    
  } catch (e) {
    await context.send("Error messaging members: " + e.message);
  }
}

/**
 * Check read count for messages
 */
async function checkReadUserCount(context) {
  const readReceiptState = getReadReceiptState(context.activity.conversation.id);
  
  if (readReceiptState.users.length !== 0 && readReceiptState.users.length !== undefined) {
    const userList = Array.from(readReceiptState.users).join(", ");
    await context.send(`Number of members read the message: ${readReceiptState.counter}\n\nMembers: ${userList}`);
  } else {
    await context.send("Read count is zero. Please make sure to send a message to all members firstly to check the count of members who have read your message.");
  }
}

/**
 * Reset the read count tracking
 */
async function resetReadUserCount(context) {
  // Reset global variables for backward compatibility
  teamMemberDetails = [];
  counter = 0;
  users = [];
  
  // Reset conversation-specific state
  if (context) {
    const readReceiptState = getReadReceiptState(context.activity.conversation.id);
    readReceiptState.teamMemberDetails = [];
    readReceiptState.counter = 0;
    readReceiptState.users = [];
    
    // Persist the updated state
    const state = getConversationState(context.activity.conversation.id);
    storage.set(context.activity.conversation.id, state);
    
    await context.send("Read count has been reset.");
  }
}

/**
 * Send a message with AI label
 */
async function addAILabel(context) {
  await context.send({
    type: 'message',
    text: `Hey I'm a friendly AI bot. This message is generated via AI`,
    entities: [
      {
        type: "https://schema.org/Message",
        "@type": "Message",
        "@context": "https://schema.org",
        additionalType: ["AIGeneratedContent"], // AI Generated label
      }
    ]
  });
}

/**
 * Send a message with sensitivity label
 */
async function addSensitivityLabel(context) {
  await context.send({
    type: 'message',
    text: `This is an example for sensitivity label that help users identify the confidentiality of a message`,
    entities: [
      {
        type: "https://schema.org/Message",
        "@type": "Message",
        "@context": "https://schema.org",
        usageInfo: {
          "@type": "CreativeWork",
          description: "Please be mindful of sharing outside of your team", // Sensitivity description
          name: "Confidential \\ Contoso FTE", // Sensitivity title
        }
      }
    ]
  });
}

/**
 * Send a message with feedback buttons enabled
 */
async function addFeedbackButtons(context) {
  await context.send({
    type: 'message',
    text: `This is an example for Feedback buttons that helps to provide feedback for a bot message`,
    channelData: {
      feedbackLoopEnabled: true // Enable feedback buttons
    },
  });
}

/**
 * Send a message with citations
 */
async function addCitations(context) {
  await context.send({
    type: 'message',
    text: `Hey I'm a friendly AI bot. This message is generated through AI [1]`, // cite with [1],
    entities: [
      {
        type: "https://schema.org/Message",
        "@type": "Message",
        "@context": "https://schema.org",
        citation: [
          {
            "@type": "Claim",
            position: 1, // Required. Must match the [1] in the text above
            appearance: {
              "@type": "DigitalDocument",
              name: "AI bot", // Title
              url: "https://example.com/claim-1", // Hyperlink on the title
              abstract: "Excerpt description", // Appears in the citation pop-up window
              text: "{\"type\":\"AdaptiveCard\",\"$schema\":\"http://adaptivecards.io/schemas/adaptive-card.json\",\"version\":\"1.6\",\"body\":[{\"type\":\"TextBlock\",\"text\":\"Adaptive Card text\"}]}", // Appears as a stringified Adaptive Card
              keywords: ["keyword 1", "keyword 2", "keyword 3"], // Appears in the citation pop-up window
              encodingFormat: "application/vnd.microsoft.card.adaptive",
              usageInfo: {
                "@type": "CreativeWork",
                name: "Confidential \\ Contoso FTE", // Sensitivity title
                description: "Only accessible to Contoso FTE", // Sensitivity description
              },
              image: {
                "@type": "ImageObject",
                name: "Microsoft Word"
              },
            },
          },
        ],
      },
    ],
  });
}

/**
 * Send a comprehensive AI message with all features
 */
async function sendAIMessage(context) {
  await context.send({
    type: 'message',
    text: `Hey I'm a friendly AI bot. This message is generated via AI [1]`,
    channelData: {
      feedbackLoopEnabled: true,
    },
    entities: [
      {
        type: "https://schema.org/Message",
        "@type": "Message",
        "@context": "https://schema.org",
        usageInfo: {
          "@type": "CreativeWork",
          "@id": "sensitivity1"
        },
        additionalType: ["AIGeneratedContent"],
        citation: [
          {
            "@type": "Claim",
            position: 1,
            appearance: {
              "@type": "DigitalDocument",
              name: "Some secret citation",
              url: "https://example.com/claim-1",
              abstract: "Excerpt",
              encodingFormat: "docx",
              keywords: ["Keyword1 - 1", "Keyword1 - 2", "Keyword1 - 3"],
              usageInfo: {
                "@type": "CreativeWork",
                "@id": "sensitivity1",
                name: "Sensitivity title",
                description: "Sensitivity description",
              },
            },
          },
        ],
      },
    ],
  });
}

module.exports = app;
