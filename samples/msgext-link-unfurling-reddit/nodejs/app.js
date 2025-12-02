const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const { ManagedIdentityCredential } = require("@azure/identity");
const config = require("./config");
const RedditOptions = require("./src/redditApi/RedditOptions");
const RedditAppAuthenticator = require("./src/redditApi/RedditAppAuthenticator");
const RedditHttpClient = require("./src/redditApi/RedditHttpClient");
const { createRedditCard } = require("./src/adaptiveCards/redditCard");

// Create storage for conversation history
const storage = new LocalStorage();

// Initialize Reddit API client
const redditOptions = new RedditOptions(config.RedditClientId, config.RedditClientSecret);
const redditAuthenticator = new RedditAppAuthenticator(redditOptions);
const redditClient = new RedditHttpClient(redditAuthenticator, redditOptions);

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
  process.env.RUNNING_IN_AZURE === "true"
    ? tokenCredentials
    : {
        clientId: config.MicrosoftAppId,
        clientSecret: config.MicrosoftAppPassword,
      };

// Create the Teams AI application
const app = new App({
  storage,
  credentials: credentialOptions,
});

// Test message handler to verify bot is working
app.on("message", async (context) => {
  const text = context.activity.text?.trim();
  
  if (text === "ping") {
    await context.send("pong! Bot is working.");
  }
});

// Handle link unfurling for Reddit links  
app.on("invoke", async (context) => {
  const name = context.activity.name;
  
  if (name === "composeExtension/queryLink") {
    try {
      const url = context.activity.value?.url;
      
      if (!url) {
        return { status: 200, body: {} };
      }

      // Check if it's a Reddit link
      if (!url.includes("reddit.com") && !url.includes("redd.it")) {
        return { status: 200, body: {} };
      }

      // Get post information from Reddit
      const postData = await redditClient.getPostInfo(url);

      // Create adaptive card
      const card = createRedditCard(postData);

      // Send the unfurling result
      return {
        status: 200,
        body: {
          composeExtension: {
            type: "result",
            attachmentLayout: "list",
            attachments: [
              {
                contentType: "application/vnd.microsoft.card.adaptive",
                content: card,
                preview: {
                  contentType: "application/vnd.microsoft.card.thumbnail",
                  content: {
                    title: postData.title,
                    text: `r/${postData.subreddit} • Posted by u/${postData.author}`,
                    images: postData.thumbnail
                      ? [{ url: postData.thumbnail }]
                      : [],
                  },
                },
              },
            ],
          },
        },
      };
    } catch (error) {
      console.error("[Link Unfurling] Error:", error);
      
      // Send error response
      return {
        status: 500,
        body: {
          error: "Failed to unfurl Reddit link",
        },
      };
    }
  }
});

// Export the app and server
module.exports = app;
