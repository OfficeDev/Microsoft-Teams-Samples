const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { TeamsBot } = require("./teamsBot");
const { ManagedIdentityCredential } = require("@azure/identity");

// Create storage for conversation history and configuration
const storage = new LocalStorage();

// Create the bot instance
const bot = new TeamsBot(storage);

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

// Handle incoming messages using the bot instance
app.on("message", async (context) => {
  await bot.handleMessage(context);
});

// Handle members added event
app.on("membersAdded", async (context) => {
  await bot.handleMembersAdded(context);
});

// Also handle conversationUpdate for when bot is added
app.on("conversationUpdate", async (context) => {
  if (context.activity.membersAdded && context.activity.membersAdded.length > 0) {
    await bot.handleMembersAdded(context);
  }
});

// Handle Teams configuration events - these are invoked when user clicks bot settings
app.on("configFetch", async (context) => {
  try {
    const result = await bot.handleTeamsConfigFetch(context, context.activity.value);
    return result;
  } catch (error) {
    console.error("Error in configFetch:", error);
    return {
      config: {
        type: 'message',
        value: 'Error loading configuration',
      },
    };
  }
});

app.on("configSubmit", async (context) => {
  try {
    const result = await bot.handleTeamsConfigSubmit(context, context.activity.value);
    return result;
  } catch (error) {
    console.error("Error in configSubmit:", error);
    return {
      config: {
        type: 'message',
        value: 'Error processing configuration',
      },
    };
  }
});

module.exports = app;
