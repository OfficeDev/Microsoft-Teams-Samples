const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const crypto = require('crypto');

// Import the request approval components
const { CardTemplates } = require("./src/cards/cardTemplates");
const { ApprovalActionHandler } = require("./src/handlers/approvalActionHandler");
const { TeamsHelper } = require("./src/services/teamsHelper");

// Deduplication: Track recent submissions to prevent duplicates
const recentSubmissions = new Map();
const processingSubmissions = new Set();
const DEDUP_WINDOW_MS = 2000; // 2 seconds window (reduced for faster response)

function isDuplicateSubmission(submitData) {
  const dataHash = crypto.createHash('md5').update(JSON.stringify(submitData)).digest('hex');
  const key = `${submitData.verb}_${dataHash}`;
  const now = Date.now();
  
  // Check if this exact submission is currently being processed
  if (processingSubmissions.has(key)) {
    console.log('🚫 Duplicate submission detected, ignoring');
    return true;
  }
  
  if (recentSubmissions.has(key)) {
    const lastSubmissionTime = recentSubmissions.get(key);
    if (now - lastSubmissionTime < DEDUP_WINDOW_MS) {
      console.log('🚫 Duplicate within window:', now - lastSubmissionTime, 'ms');
      return true;
    }
  }
  
  processingSubmissions.add(key);
  recentSubmissions.set(key, now);
  
  // Cleanup old entries
  for (const [k, timestamp] of recentSubmissions.entries()) {
    if (now - timestamp > DEDUP_WINDOW_MS) {
      recentSubmissions.delete(k);
    }
  }
  
  return false;
}

function markSubmissionComplete(submitData) {
  const dataHash = crypto.createHash('md5').update(JSON.stringify(submitData)).digest('hex');
  const key = `${submitData.verb}_${dataHash}`;
  processingSubmissions.delete(key);
}

// Create storage for conversation history and task management
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

const credentialOptions = config.MicrosoftAppType === "UserAssignedMsi" ? { ...tokenCredentials } : undefined;

// Create the app with storage
const app = new App({
  ...credentialOptions,
  storage,
});

// Initialize the approval action handler
const approvalHandler = new ApprovalActionHandler(storage);

// Handle adaptive card actions - optimized for immediate response
app.on("adaptiveCard/action", async (context) => {
  try {
    console.log('📥 Adaptive card action triggered');
    
    const action = context.activity.value?.action || context.activity.value;
    if (!action) {
      console.error('❌ No action data found in activity');
      return CardTemplates.getOptionsCard();
    }
    
    console.log('🎯 Processing action:', action.verb);
    
    // Process action immediately and return response card
    const responseCard = await approvalHandler.handleAction(context, action);
    
    // Return the card immediately to minimize delay
    return responseCard;
  } catch (error) {
    console.error('❌ Error handling adaptive card action:', error);
    
    // Return error card immediately
    return {
      $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
      type: 'AdaptiveCard',
      version: '1.4',
      body: [
        {
          type: 'TextBlock',
          size: 'Medium',
          weight: 'Bolder',
          text: 'Task Management'
        },
        {
          type: 'TextBlock',
          text: 'Something went wrong. Please try again.',
          color: 'Attention',
          wrap: true
        },
        {
          type: 'TextBlock',
          text: `Error: ${error.message || 'Unknown error occurred'}`,
          size: 'Small',
          wrap: true,
          isSubtle: true
        }
      ],
      actions: [
        {
          type: 'Action.Execute',
          verb: 'back_to_options',
          title: 'Back to Options',
          data: { info: 'back' }
        }
      ]
    };
  }
});

// Handle message events
app.on("message", async (context) => {
  try {
    console.log('💬 Message received from:', context.activity?.from?.name || 'unknown');
    
    // Debug: Log the available context properties
    console.log('🔍 Available context keys:', Object.keys(context));
    if (context.api) {
      console.log('🔍 API available:', typeof context.api);
      console.log('🔍 API keys:', Object.keys(context.api));
      if (context.api.conversations) {
        console.log('🔍 Conversations API available:', typeof context.api.conversations);
      }
    }
    
    const activity = context.activity;
    
    // Check if this is an adaptive card submit action
    if (activity.value && typeof activity.value === 'object' && activity.value.verb) {
      // Check for duplicate submissions
      if (isDuplicateSubmission(activity.value)) {
        console.log('🚫 Duplicate submission detected, ignoring:', activity.value.verb);
        return;
      }
      
      try {
        console.log('🎯 Processing submit action:', activity.value.verb);
        
        const action = {
          verb: activity.value.verb,
          data: activity.value
        };
        
        // Process the action and get response immediately
        const responseCard = await approvalHandler.handleAction(context, action);
        
        // Send response immediately if we have one
        if (responseCard) {
          await context.send(responseCard);
        }
        
        markSubmissionComplete(activity.value);
        return;
        
      } catch (error) {
        console.error('❌ Error handling submit action:', error);
        markSubmissionComplete(activity.value);
        
        // Send error response immediately
        await context.send({
          $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
          type: 'AdaptiveCard',
          version: '1.4',
          body: [
            {
              type: 'TextBlock',
              text: `Error: ${error.message}`,
              color: 'Attention'
            }
          ]
        });
        return;
      }
    }
    
    const text = stripMentionsText(activity);
    console.log('📝 Message text:', text);

    // Command to start the request approval workflow
    if (text.toLowerCase() === "request" || text.toLowerCase() === "/request") {
      console.log('🚀 Starting request approval workflow');
      await context.send(CardTemplates.getOptionsCard());
      return;
    }

    // Direct command to create new task
    if (["create task", "/create", "create"].includes(text.toLowerCase())) {
      console.log('🚀 Creating task directly via command');
      try {
        const members = await TeamsHelper.getConversationMembers(context);
        const currentUser = activity.from;
        const assigneeOptions = TeamsHelper.createAssigneeOptions(members, currentUser.id);
        const createTaskCard = CardTemplates.getCreateTaskCard(currentUser, assigneeOptions);
        
        await context.send(createTaskCard);
        return;
      } catch (error) {
        console.error('❌ Error creating task card:', error);
        await context.send(`❌ Error creating task: ${error.message}`);
        return;
      }
    }
  } catch (error) {
    console.error('❌ Error in message handler:', error);
  }

      // Development and debug commands
    const debugCommands = {
      "/reset": () => {
        context.storage.conversations.delete();
        return "Ok I've deleted the current conversation state.";
      },
      "/count": async () => {
        const state = await context.storage.conversations.get();
        return `The count is ${state.count}`;
      },
      "/diag": () => JSON.stringify(activity),
      "/runtime": () => JSON.stringify({
        nodeversion: process.version,
        sdkversion: "2.0.0",
        features: ["request-approval", "adaptive-cards", "task-management"]
      })
    };
    
    if (debugCommands[text]) {
      const response = await debugCommands[text]();
      await context.send(response);
      return;
    }

  if (text === "/help") {
    await context.send(`**Request Approval Bot Commands:**
    
• **request** - Show task management options
• **create task** or **create** - Create a new task directly
• **help** - Show this help message

**Development Commands:**
• **/reset** - Clear conversation state
• **/count** - Show message count
• **/state** - Show conversation state
• **/diag** - Show activity details
• **/runtime** - Show runtime information

**How to use:**
1. Type **request** to see options, or **create task** to start directly
2. Fill in the task details (title, description, assignee)
3. The assignee will receive approval/rejection options
4. Only you can edit or cancel your requests
5. Only the assignee can approve or reject requests`);
    return;
  }

    // Default welcome/help behavior for any other message
    const welcomeMessage = `👋 Welcome to the **Request Approval Bot**! 

I help you create and manage task approval requests in your team chats.

**Quick Commands:**
• Type **create task** to start creating a new task immediately
• Type **request** to see all options
• Type **help** for more commands

**Key Features:**
• 📝 Create detailed task requests with titles and descriptions
• 👥 Assign tasks to specific team members  
• ✅ Approve or reject requests with one click
• ✏️ Edit or cancel your own requests
• 🔄 Real-time card updates for all team members`;

    await context.send(welcomeMessage);
});

// Handle member additions (when bot is added to a team/chat)
app.on("membersAdded", async (context) => {
  const membersAdded = context.activity.membersAdded;
  for (let member of membersAdded) {
    if (member.id !== context.activity.recipient.id) {
      await context.send(`🎉 Welcome to the **Request Approval Bot**!

I help streamline task approvals in your team. Here's how:

• **Requesters** can create approval requests using **request** command
• **Managers/Assignees** can approve or reject requests directly in the chat  
• **Team Members** can view request details

Type **request** to get started, or **help** for all available commands!`);
    }
  }
});

module.exports = app;
