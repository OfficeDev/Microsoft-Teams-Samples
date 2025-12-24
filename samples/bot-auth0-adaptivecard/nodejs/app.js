const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const TokenStore = require('./services/TokenStore');
const fetch = require('node-fetch');
const express = require('express');

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

// Helper function to generate login URL
const generateLoginUrl = (userId) => {
  return `https://${process.env.AUTH0_DOMAIN}/authorize` +
    `?response_type=code&client_id=${process.env.AUTH0_CLIENT_ID}` +
    `&redirect_uri=${process.env.APP_URL}/api/auth/callback` +
    `&scope=openid profile email` +
    `&state=${encodeURIComponent(userId)}`;
};

app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity)?.trim().toLowerCase();
  const userId = activity.from.id;

  // Handle logout
  if (text === 'logout') {
    TokenStore.removeToken(userId);
    const logoutUrl = `https://${process.env.AUTH0_DOMAIN}/v2/logout?client_id=${process.env.AUTH0_CLIENT_ID}`;
    
    const logoutCard = {
      type: "AdaptiveCard",
      version: "1.3",
      body: [
        {
          type: "TextBlock",
          text: "You've been logged out.",
          size: "Medium",
          weight: "Bolder",
          wrap: true
        }
      ],
      actions: [
        {
          type: "Action.OpenUrl",
          title: "Logout from Auth0",
          url: logoutUrl
        }
      ]
    };

    await context.send({
      type: 'message',
      attachments: [{
        contentType: 'application/vnd.microsoft.card.adaptive',
        content: logoutCard
      }]
    });
    return;
  }

  // Check if user is authenticated
  const accessToken = TokenStore.getToken(userId);
  
  if (accessToken) {
    // Handle profile details request
    if (text === 'profile details') {
      try {
        const response = await fetch(`https://${process.env.AUTH0_DOMAIN}/userinfo`, {
          method: 'GET',
          headers: {
            Authorization: `Bearer ${accessToken}`,
          },
        });

        if (response.ok) {
          const profileData = await response.json();

          const profileCard = {
            type: "AdaptiveCard",
            version: "1.3",
            body: [
              {
                type: "TextBlock",
                text: `Auth0 Profile`,
                size: "Large",
                weight: "Bolder",
                wrap: true
              },
              {
                type: "Image",
                url: profileData.picture || "https://via.placeholder.com/150",
                size: "Medium",
                style: "Person"
              },
              {
                type: "TextBlock",
                text: `Name: ${profileData.name}`,
                wrap: true
              },
              {
                type: "TextBlock",
                text: `Email: ${profileData.email}`,
                wrap: true
              }
            ]
          };

          await context.send({
            type: 'message',
            attachments: [{
              contentType: 'application/vnd.microsoft.card.adaptive',
              content: profileCard
            }]
          });
        } else {
          await context.send('Failed to fetch profile details.');
        }
      } catch (err) {
        console.error('Error fetching profile:', err);
        await context.send('Error retrieving profile details.');
      }
    } else {
      await context.send("Say 'profile details' to get your profile or 'logout' to log out.");
    }
  } else {
    // User not authenticated - prompt for login
    const loginUrl = generateLoginUrl(userId);

    const loginCard = {
      type: "AdaptiveCard",
      version: "1.3",
      body: [
        {
          type: "TextBlock",
          text: "Login Required",
          size: "Medium",
          weight: "Bolder",
          wrap: true
        }
      ],
      actions: [
        {
          type: "Action.OpenUrl",
          title: "Login",
          url: loginUrl
        }
      ]
    };

    await context.send({
      type: 'message',
      attachments: [{
        contentType: 'application/vnd.microsoft.card.adaptive',
        content: loginCard
      }]
    });
  }
});

// Setup custom routes
setupCustomRoutes();

function setupCustomRoutes() {
  const expressApp = app.http.express;

  if (expressApp) {
    const path = require('path');
    const authRouter = require('./controllers/AuthController');

    // Use body parsing middleware
    expressApp.use(express.json());
    expressApp.use(express.urlencoded({ extended: true }));

    // Serve static files (e.g., auth-end.html)
    expressApp.use('/public', express.static(path.join(__dirname, 'public')));

    // Use the auth router
    expressApp.use('/api/auth', authRouter);
  }
}

module.exports = app;
