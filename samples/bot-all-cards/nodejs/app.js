const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const path = require('path');
const dotenv = require('dotenv');

// Import card templates
const AdaptiveCard = require('./resources/adaptiveCard.json');
const Office365ConnectorCard = require('./resources/o365ConnectorCard.json');
const ThumbnailCard = require('./resources/thumbnailCard.json');
const ListCard = require('./resources/listCard.json');
const CollectionCard = require('./resources/collectionsCard.json');

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
  await context.send('Welcome to Cards. This bot will introduce you to different types of cards. Please select the cards from given options.');
  await sendSuggestedCards(context);
});

/**
 * Handle message events
 */
app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity);

  // Create an array with the valid card options
  const suggestedCards = ['AdaptiveCard', 'HeroCard', 'ListCard', 'Office365', 'CollectionCard', 'SignIn', 'OAuth', 'ThumbnailCard'];


  // If the text is in the Array, a valid card was selected
  if (suggestedCards.includes(text)) {
    switch (text) {
      case 'AdaptiveCard':
        try {
          await context.send({
            type: 'message',
            attachments: [{
              contentType: 'application/vnd.microsoft.card.adaptive',
              content: AdaptiveCard
            }]
          });
        } catch (error) {
          console.error("Error sending AdaptiveCard:", error);
          await context.send("Error sending adaptive card: " + error.message);
        }
        break;

      case 'HeroCard':
        await context.send({
          type: 'message',
          attachments: [{
            contentType: 'application/vnd.microsoft.card.hero',
            content: {
              title: 'BotFramework Hero Card',
              images: [{ 
                url: 'https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Seattle_monorail01_2008-02-25.jpg/1024px-Seattle_monorail01_2008-02-25.jpg' 
              }],
              buttons: [{ 
                type: 'openUrl',
                title: 'Get started',
                value: 'https://docs.microsoft.com/en-us/azure/bot-service/'
              }]
            }
          }]
        });
        break;

      case 'Office365':
        await context.send({
          type: 'message',
          attachments: [{
            contentType: 'application/vnd.microsoft.teams.card.o365connector',
            content: Office365ConnectorCard
          }]
        });
        break;

      case 'ThumbnailCard':
        await context.send({
          type: 'message',
          attachments: [ThumbnailCard]
        });
        break;

      case 'SignIn':
        await context.send({
          type: 'message',
          attachments: [{
            contentType: 'application/vnd.microsoft.card.signin',
            content: {
              text: 'BotFramework SignIn Card',
              buttons: [{ 
                type: 'signin',
                title: 'Sign In',
                value: 'https://login.microsoftonline.com'
              }]
            }
          }]
        });
        break;

      case 'OAuth':
        await context.send({
          type: 'message',
          attachments: [{
            contentType: 'application/vnd.microsoft.card.oauth',
            content: {
              text: 'BotFramework OAuth Card',
              connectionName: process.env.ConnectionName, // Your Azure bot connection name
              buttons: [{ 
                type: 'signin',
                title: 'Sign In',
                value: 'signin'
              }]
            }
          }]
        });
        break;

      case 'ListCard':
        await context.send({
          type: 'message',
          attachments: [ListCard]
        });
        break;

      case 'CollectionCard':
        await context.send({
          type: 'message',
          attachments: [{
            contentType: 'application/vnd.microsoft.card.adaptive',
            content: CollectionCard
          }]
        });
        break;
    }

    await context.send(`You have selected <b>${text}</b>`);
  }

  // After the bot has responded send the suggested Cards
  await sendSuggestedCards(context);
});

/**
 * Send suggested Cards to the user.
 * @param {Object} context The turn context.
 */
async function sendSuggestedCards(context) {
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.hero',
      content: {
        title: 'Please select a card from given options.',
        buttons: [
          {
            type: 'imBack',
            title: 'AdaptiveCard',
            value: 'AdaptiveCard'
          },
          {
            type: 'imBack',
            title: 'HeroCard',
            value: 'HeroCard'
          },
          {
            type: 'imBack',
            title: 'ListCard',
            value: 'ListCard'
          },
          {
            type: 'imBack',
            title: 'Office365',
            value: 'Office365'
          },
          {
            type: 'imBack',
            title: 'CollectionCard',
            value: 'CollectionCard'
          },
          {
            type: 'imBack',
            title: 'SignIn',
            value: 'SignIn'
          },
          {
            type: 'imBack',
            title: 'ThumbnailCard',
            value: 'ThumbnailCard'
          }
        ]
      }
    }]
  });
}

module.exports = app;
