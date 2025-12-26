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


/**
 * Handle app installation events to send welcome message
 */
app.on("install.add", async (context) => {
  await context.send('Welcome to Adaptive Card Action and Suggested Action Bot. This bot will introduce you to suggested actions. Please select an option:');
  await sendAdaptiveCardActions(context);
});

/**
 * Handle message events
 */
app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity);

  const adaptiveActionCards = {
    'CardActions': () => createAdaptiveCardAttachment(getAdaptiveCardActions()),
    'SuggestedActions': () => createAdaptiveCardAttachment(getSuggestedActionsCard()),
    'ToggleVisibility': () => createAdaptiveCardAttachment(getToggleVisibilityCard()),
  };

  // Handle card selection
  if (adaptiveActionCards[text]) {
    try {
      const attachment = adaptiveActionCards[text]();
      await context.send({
        type: 'message',
        attachments: [attachment]
      });
      await context.send(`You have Selected <b>${text}</b>`);
      
      if (text === 'SuggestedActions') {
        await sendSuggestedActions(context);
      }
    } catch (error) {
      console.error(`Error sending ${text} card:`, error);
      await context.send(`Error sending ${text} card: ${error.message}`);
    }
    return; 
  } 
  // Handle color selection responses
  else if (["Red", "Blue", "Yellow"].includes(text)) {
    await context.send(`I agree, ${text} is the best color.`);
    await sendSuggestedActions(context);
    return; 
  }
  // Handle form submission (adaptive card submissions)
  else if (activity.value != null && activity.text == undefined) {
    const activityValue = activity.value;
    if (activityValue.hasOwnProperty('name')) {
      await context.send(`Data Submitted: ${activityValue.name}`);
    }
    return;
  }
  // Handle unrecognized commands
  else if (text && !adaptiveActionCards[text]) {
    await context.send("Please use one of these commands: **CardActions** for Adaptive Card Actions, **SuggestedActions** for Bot Suggested Actions, and **ToggleVisibility** for Action ToggleVisible Card");
  }
});

/**
 * Creates an adaptive card attachment
 * @param {Object} cardJson The adaptive card JSON
 * @returns {Object} The attachment object
 */
function createAdaptiveCardAttachment(cardJson) {
  return {
    contentType: 'application/vnd.microsoft.card.adaptive',
    content: cardJson
  };
}

/**
 * Send Adaptive Card Action options to the user.
 * @param {Object} context The turn context.
 */
async function sendAdaptiveCardActions(context) {
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.hero',
      content: {
        title: 'Please select a card from given options.',
        buttons: [
          { type: 'imBack', title: 'Card Actions', value: 'CardActions' },
          { type: 'imBack', title: 'Suggested Actions', value: 'SuggestedActions' },
          { type: 'imBack', title: 'Toggle Visibility', value: 'ToggleVisibility' }
        ]
      }
    }]
  });
}

/**
 * Send suggested actions to the user for color selection.
 * @param {Object} context The turn context.
 */
async function sendSuggestedActions(context) {
    const reply = {
        type: 'message',
        text: "What is your favorite color?",
        suggestedActions: {
            actions: [
                { type: 'imBack', title: 'Red', value: 'Red' },
                { type: 'imBack', title: 'Yellow', value: 'Yellow' },
                { type: 'imBack', title: 'Blue', value: 'Blue' }
            ],
            to: [context.activity.from.id]
        }
    };
    await context.send(reply);
}

/**
 * Returns the adaptive card actions.
 * @returns {Object} The adaptive card actions.
 */
function getAdaptiveCardActions() {
    return {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.0",
        "body": [{ "type": "TextBlock", "text": "Adaptive Card Actions" }],
        "actions": [
            { "type": "Action.OpenUrl", "title": "Action Open URL", "url": "https://adaptivecards.io" },
            {
                "type": "Action.ShowCard", "title": "Action Submit", "card": {
                    "type": "AdaptiveCard", "version": "1.5",
                    "body": [{ "type": "Input.Text", "id": "name", "label": "Please enter your name:", "isRequired": true, "errorMessage": "Name is required" }],
                    "actions": [{ "type": "Action.Submit", "title": "Submit" }]
                }
            },
            {
                "type": "Action.ShowCard", "title": "Action ShowCard", "card": {
                    "type": "AdaptiveCard", "version": "1.0",
                    "body": [{ "type": "TextBlock", "text": "This card's action will show another card" }],
                    "actions": [{
                        "type": "Action.ShowCard", "title": "Action.ShowCard", "card": {
                            "type": "AdaptiveCard",
                            "body": [
                                { "type": "TextBlock", "text": "**Welcome To New Card**" },
                                { "type": "TextBlock", "text": "This is your new card inside another card" }
                            ]
                        }
                    }]
                }
            }
        ]
    };
}

/**
 * Returns the toggle visibility card.
 * @returns {Object} The toggle visibility card.
 */
function getToggleVisibilityCard() {
    return {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.0",
        "body": [
            { "type": "TextBlock", "text": "**Action.ToggleVisibility example**: click the button to show or hide a welcome message" },
            { "type": "TextBlock", "id": "helloWorld", "isVisible": false, "text": "**Hello World!**", "size": "extraLarge" }
        ],
        "actions": [{ "type": "Action.ToggleVisibility", "title": "Click me!", "targetElements": ["helloWorld"] }]
    };
}

/**
 * Returns the suggested actions card.
 * @returns {Object} The suggested actions card.
 */
function getSuggestedActionsCard() {
    return {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.0",
        "body": [
            { "type": "TextBlock", "text": "**Welcome to bot Suggested actions** please use below commands." },
            { "type": "TextBlock", "text": "please use below commands, to get response form the bot." },
            { "type": "TextBlock", "text": "- Red \r- Blue \r - Yellow", "wrap": true }
        ]
    };
}

module.exports = app;
