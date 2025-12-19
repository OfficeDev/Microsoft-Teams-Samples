const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const TeamsBot = require("./teamsBot");

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

// Create bot instance
const bot = new TeamsBot();

// Handle message activities
app.on("message", async (context) => {
  await bot.handleMessage(context);
});

// Handle conversation update activities (member added)
app.on("conversationUpdate", async (context) => {
  await bot.handleConversationUpdate(context);
});

// Handle all invoke activities
app.on("invoke", async (context) => {
  if (context.activity.name === "config/fetch") {
    const response = await bot.handleConfigFetch(context);
    
    // For invoke activities, return the response with status code
    return {
      status: 200,
      body: response
    };
  }
  
  if (context.activity.name === "config/submit") {
    const response = await bot.handleConfigSubmit(context);
    
    return {
      status: 200,
      body: response
    };
  }
});

// Handle config/submit invoke activities
app.on("config/submit", async (context) => {
  const response = await bot.handleConfigSubmit(context);
  await context.sendInvokeResponse(200, response);
});

module.exports = app;
