const { stripMentionsText, MessageActivity } = require("@microsoft/teams.api");
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

app.on("message", async ({ activity, send }) => {
  const text = stripMentionsText(activity).trim();

  if (text === 'Hello'|| text==='hello') {
    // Only send the mention message, no card
    await mentionActivityAsync({ activity, send });
    return; // Exit early, don't continue
  }
  
  // For all other messages, show the card
  await send({
    type: "AdaptiveCard",
    version: "1.5",
    body: [
      {
        type: "TextBlock",
        text: "Lets talk...",
        wrap: true,
        weight: "Bolder"
      }
    ],
    actions: [
      {
        type: "Action.Submit",
        title: "Say Hello",
        data: {
          msteams: {
            type: "messageBack",
            displayText: "Say Hello",
            text: "Hello",
            value: { count: 0 }
          }
        }
      }
    ]
  });
});

/**
 * Say hello and @ mention the current user.
 */
async function mentionActivityAsync({ activity, send }) {
  await send(new MessageActivity('Hi!').addMention(activity.from));
}

module.exports = app;
