const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const { GetTranslatedRes } = require('./services/languageService');
const path = require('path');

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
    try {
        // Remove the mention of the bot in the received message (Teams AI v2 equivalent)
        const messageText = stripMentionsText(activity.text || "");

        // Get the locale of the activity (user's language preference)
        const locale = activity.locale || 'en-us'; // Default to 'en-us' if locale is not provided

        // Get the translation for the welcome message based on locale
        const text = GetTranslatedRes(locale).welcome;

        // Send the welcome message to the user (Teams AI v2 method)
        await send(text);

    } catch (error) {
        // Handle errors gracefully (e.g., missing translation files)
        console.error(`Error handling message: ${error.message}`);
        const defaultText = GetTranslatedRes('en-us').welcome; // Fallback to English if an error occurs
        await send(defaultText);
    }
});

// Handle members added events (equivalent to TeamsActivityHandler.onMembersAdded)
app.on("membersAdded", async ({ activity, send }) => {
    try {
        const locale = activity.locale || 'en-us';
        const text = GetTranslatedRes(locale).welcome;
        
        const membersAdded = activity.membersAdded;
        for (const member of membersAdded) {
            if (member.id !== activity.recipient.id) {
                await send(text);
            }
        }
    } catch (error) {
        console.error(`Error handling members added: ${error.message}`);
        const defaultText = GetTranslatedRes('en-us').welcome;
        await send(defaultText);
    }
});

// Host static files for the localization demo tab
app.tab('localization', path.resolve(__dirname, 'static'));

module.exports = app;
