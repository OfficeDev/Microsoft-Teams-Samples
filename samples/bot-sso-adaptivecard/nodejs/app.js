const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const { ConsoleLogger } = require("@microsoft/teams.common/logging");
const endpoints = require('@microsoft/teams.graph-endpoints');
const ACData = require("adaptivecards-templating");

// Load adaptive card templates
const AdaptiveCardResponse = require('./resources/adaptiveCardResponseJson.json');
const AdaptiveCardWithSSO = require('./resources/AdaptiveCardWithSSOInRefresh.json');
const Options = require('./resources/options.json');

// Create storage for conversation history
const storage = new LocalStorage();

// Create the app with storage and OAuth configuration
const app = new App({
  storage,
  oauth: {
    defaultConnectionName: process.env.CONNECTION_NAME || "oauthbotsetting",
  },
  logger: new ConsoleLogger("bot-sso-adaptivecard", { level: "error" }),
});

// Send welcome message when app is installed
app.on("install.add", async ({ send }) => {
  await send("Welcome to Universal Adaptive Cards. Type 'login' to get sign in universal sso.");
});

// Handle 'login' message - send the initial adaptive card with "Sign in Universal SSO" button
app.message('login', async ({ send }) => {
  await send({
    type: 'message',
    text: '',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.adaptive',
      content: Options
    }]
  });
});

// Handle 'PerformSSO' message - send the SSO adaptive card
app.message('PerformSSO', async ({ send }) => {
  await send({
    type: 'message',
    text: '',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.adaptive',
      content: AdaptiveCardWithSSO
    }]
  });
});

// Handle successful sign-in event
app.event('signin', async ({ send, token, userGraph }) => {
  try {
    // Get user profile
    const me = await userGraph.call(endpoints.me.get);
    
    // Create adaptive card template
    const template = new ACData.Template(AdaptiveCardResponse);
    const payloadData = {
      authresult: `SSO success: Welcome ${me.displayName}!`
    };
    const cardPayload = template.expand(payloadData);
    
    // Send the success card
    await send({
      type: 'message',
      text: '',
      attachments: [{
        contentType: 'application/vnd.microsoft.card.adaptive',
        content: cardPayload
      }]
    });
  } catch (error) {
    console.error('Error in signin event:', error);
    await send("Authentication successful, but couldn't fetch user profile.");
  }
});

// Handle all invoke activities to catch adaptive card actions
app.on('invoke', async ({ activity, token, userGraph }) => {
  if (activity.name === 'adaptiveCard/action') {
    const verb = activity.value?.action?.verb;
    const authentication = activity.value?.authentication;
    
    // Check if we have authentication token (after SSO completes)
    if (authentication?.token) {
      try {
        // Try to get user info using the SDK's userGraph which handles token exchange
        const me = await userGraph.call(endpoints.me.get);
        
        // Create success card with user name
        const template = new ACData.Template(AdaptiveCardResponse);
        const payloadData = {
          authresult: `You are successfully logged-in as ${me.displayName}!`
        };
        const cardPayload = template.expand({ $root: payloadData });
        
        return {
          statusCode: 200,
          type: 'application/vnd.microsoft.card.adaptive',
          value: cardPayload
        };
      } catch (error) {
        // If Graph call fails, just show success without user name
        const template = new ACData.Template(AdaptiveCardResponse);
        const payloadData = {
          authresult: 'You are successfully logged-in'
        };
        const cardPayload = template.expand({ $root: payloadData });
        
        return {
          statusCode: 200,
          type: 'application/vnd.microsoft.card.adaptive',
          value: cardPayload
        };
      }
    }
    
    // Handle initiateSSO - first time the card loads
    if (verb === 'initiateSSO') {
      // Return null to let SDK handle SSO flow
      return null;
    }
    
    if (verb === 'basicRefresh') {
      const template = new ACData.Template(AdaptiveCardResponse);
      const payloadData = {
        authresult: 'Refreshed successfully'
      };
      const cardPayload = template.expand(payloadData);
      
      return {
        statusCode: 200,
        type: 'application/vnd.microsoft.card.adaptive',
        value: cardPayload
      };
    }
  }
  
  return null;
});

// Handle default messages
app.on('message', async ({ send, activity }) => {
  const text = activity.text?.toLowerCase();
  if (!text || text === 'login' || text === 'performsso') {
    return; // Already handled by specific handlers
  }
  await send("Please send 'login' for options");
});

module.exports = app;
