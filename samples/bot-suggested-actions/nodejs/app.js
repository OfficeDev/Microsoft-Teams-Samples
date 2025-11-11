const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage} = require("@microsoft/teams.common");
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

// Handle members added event
app.on("membersAdded", async (context) => {
  await sendWelcomeMessage(context);
});

/**
 * Sends a welcome message along with suggested actions.
 * @param {*} context - Teams AI SDK context
 */
async function sendWelcomeMessage(context) {
  const { activity } = context;

  // Iterate over all new members added to the conversation.
  for (const member of activity.membersAdded) {
    if (member.id !== activity.recipient.id) {
      const welcomeMessage = 'Welcome to the suggested actions bot. This bot will introduce you to suggested actions. Please select an option:';
      await context.send(welcomeMessage);
      await sendSuggestedActions(context);
    }
  }
}

/**
 * Sends suggested actions to the user.
 * @param {*} context - Teams AI SDK context
 */
async function sendSuggestedActions(context) {
  const cardActions = [
    { type: "imBack", title: 'Hello', value: 'Hello' },
    { type: "imBack", title: 'Welcome', value: 'Welcome' },
    {
      type: "Action.Compose",
      title: "@SuggestedActionsBot",
      value: {
        type: "Teams.chatMessage",
        data: {
          body: {
            additionalData: {},
            backingStore: {
              returnOnlyChangedValues: false,
              initializationCompleted: true
            },
            content: "<at id=\"0\">SuggestedActionsBot</at>"
          },
          mentions: [
            {
              additionalData: {},
              backingStore: {
                "returnOnlyChangedValues": false,
                "initializationCompleted": false
              },
              id: 0,
              mentioned: {
                additionalData: {},
                backingStore: {
                  returnOnlyChangedValues: false,
                  initializationCompleted: false
                },
                odataType: "#microsoft.graph.chatMessageMentionedIdentitySet",
                user: {
                  additionalData: {},
                  backingStore: {
                    returnOnlyChangedValues: false,
                    initializationCompleted: false
                  },
                  displayName: "Suggested Actions Bot",
                  id: "28:" + (process.env.MICROSOFT_APP_ID || process.env.CLIENT_ID),
                }
              },
              mentionText: "Suggested Actions Bot"
            }
          ],
          additionalData: {},
          backingStore: {
            returnOnlyChangedValues: false,
            initializationCompleted: true
          }
        }
      }
    }
  ];

  const reply = {
    type: "message",
    text: 'Choose one of the action from the suggested action',
    suggestedActions: { 
      actions: cardActions, 
      to: [context.activity.from.id] 
    }
  };
  
  await context.send(reply);
}

app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity);

  // Handle specific messages with suggested actions
  switch(text) {
    case 'Hello':
      await context.send('Hello! How can I assist you today?');
      break;
    case 'Welcome':
      await context.send('Welcome! How can I assist you today?');
      break;
    default:
      // Default echo behavior
      const state = getConversationState(activity.conversation.id);
      state.count++;
      await context.send(`[${state.count}] you said: ${text}`);
      await context.send('Please select one action.');
  }

  // Send suggested actions after responding
  await sendSuggestedActions(context);
});

module.exports = app;
