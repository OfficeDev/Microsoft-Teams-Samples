const path = require('path');
const express = require('express');

// Load environment variables from Teams Toolkit environment files
const TEAMSFX_ENV = process.env.TEAMSFX_ENV || 'local';
require('dotenv').config({ path: path.join(__dirname, 'env', `.env.${TEAMSFX_ENV}`) });
require('dotenv').config({ path: path.join(__dirname, 'env', `.env.${TEAMSFX_ENV}.user`) });

const PORT = process.env.PORT || 3978;

// Import Teams AI components
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

// Create the Teams AI app
const app = new App({
  ...credentialOptions,
  storage,
});

// Setup custom routes immediately after app creation
setupCustomRoutes();

// Setup custom routes on the Teams AI Express app
function setupCustomRoutes() {
  const expressApp = app.http.express;
  
  if (expressApp) {
    // Configure EJS and static files
    expressApp.set('view engine', 'ejs');
    expressApp.set('views', path.join(__dirname, 'views'));
    expressApp.use('/static', express.static(path.join(__dirname, 'static')));
    expressApp.use('/Images', express.static(path.join(__dirname, 'Images')));
    
    // Add dedicated link unfurling route (since Teams AI controls the root route)
    expressApp.get('/linkunfurling', (req, res) => {
      res.redirect('/tab?openInTeams=false');
    });
    
    // Add tab route
    expressApp.get('/tab', (req, res) => {
      try {
        const botEndpoint = process.env.BOT_ENDPOINT;
        res.render('tab', {
          linkUrl: req.query.url ,
          previewData: {
            title: 'Link Unfurling Example',
            description: 'This is a preview of the shared link',
            image: `${botEndpoint}/Images/preview.png`,
            url: req.query.url 
          },
          botEndpoint: botEndpoint
        });
      } catch (error) {
        res.status(500).json({ error: 'Error rendering tab view', details: error.message });
      }
    });
    
  } else {
  }
}

// Teams Bot functionality using Teams AI v2
class TeamsBot {
  constructor(app) {
    this.app = app;
    this.setupHandlers();
  }

  setupHandlers() {
    // Handle app-based link query activity (link unfurling) - exact match to nodejs handleTeamsAppBasedLinkQuery
    // Try multiple event names to ensure link unfurling works
    this.app.on("composeExtensions/queryLink", async (context) => {
      return this.handleLinkUnfurling(context);
    });

    this.app.on("composeExtensions/query", async (context) => {
      const query = context.activity.value;
      
      // Check if this is a link unfurling query
      if (query && query.url) {
        return this.handleLinkUnfurling(context);
      }
      
      // Default query response
      return {
        composeExtension: {
          type: 'result',
          attachmentLayout: 'list',
          attachments: []
        }
      };
    });

    // Handle tab/tabInfoAction invoke
    this.app.on("invoke", async (context) => {
      const activity = context.activity;
      
      if (activity.name === "tab/tabInfoAction") {
        return {
          statusCode: 200,
          type: "continue"
        };
      }
      
      return {
        statusCode: 200
      };
    });
  }

  // Handle link unfurling (replicating nodejs handleTeamsAppBasedLinkQuery behavior)
  handleLinkUnfurling(context) {
    const query = context.activity.value;
    const url = query?.url;
    
    var userCard = this.getLinkUnfurlingCard();
    const preview = {
      contentType: "application/vnd.microsoft.card.thumbnail",
      content: {
        title: 'Adaptive Card',
        text: 'Please select to get the card'
      }
    };

    return {
      composeExtension: {
        attachmentLayout: 'list',
        type: 'result',
        attachments: [{
          contentType: "application/vnd.microsoft.card.adaptive",
          content: userCard,
          preview: preview
        }]
      }
    };
  }

  // Adaptive card for link unfurling (exact copy from nodejs teamsBot.js)
  getLinkUnfurlingCard() {
    // Use same environment variable names as nodejs version for compatibility
    const ApplicationBaseUrl = process.env.BOT_ENDPOINT || process.env.ApplicationBaseUrl || config.BOT_ENDPOINT;
    const MicrosoftAppId = process.env.TEAMS_APP_ID || process.env.MicrosoftAppId || config.TEAMS_APP_ID;
    
    var card = {
      "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
      "type": "AdaptiveCard",
      "version": "1.4",
      "body": [
        {
          "type": "TextBlock",
          "size": "Medium",
          "weight": "Bolder",
          "text": "Analytics details:",
          "horizontalAlignment": "Center"
        },
        {
          "type": "Image",
          "url": `${ApplicationBaseUrl}/Images/report.png`,
          "altText": "Analytics Report",
          "size": "Medium",
          "horizontalAlignment": "Center"
        }
      ],
      "actions":[
        {
            "type": "Action.OpenUrl",
            "title": "Open tab",
            "url": `https://teams.microsoft.com/l/entity/${MicrosoftAppId}/tab?webUrl=${ApplicationBaseUrl}/tab?openInTeams=true`
          },
          {
            "type": "Action.Submit",
            "title": "View via card",
            "data": {
                "msteams": {
                    "type": "invoke",
                    "value": {
                        "type": "tab/tabInfoAction",
                        "tabInfo": {
                            "contentUrl":  ApplicationBaseUrl + "/tab?openInTeams=true",
                            "websiteUrl": ApplicationBaseUrl + "/tab?openInTeams=true",
                            "name": "Stage view",
                            "entityId": "entityId"
                         }
                        }
                    }
            }
        }
      ]
    }
  
    return card;
  }
}

// Create bot instance
const bot = new TeamsBot(app);

// Start the application (exact pattern from nodejs)
(async () => {
  try {
    await app.start(PORT);
    
    const botEndpoint = process.env.BOT_ENDPOINT;
    
    // Handle graceful shutdown
    process.on('SIGINT', async () => {
      process.exit(0);
    });
    
  } catch (error) {
    process.exit(1);
  }
})();

// Export the app for external use if needed
module.exports = app;
