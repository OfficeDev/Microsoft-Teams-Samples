const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const express = require("express");
const cors = require("cors");
const { MicrosoftAppCredentials } = require('botframework-connector');

// Create bot credentials 
const credentials = new MicrosoftAppCredentials(process.env.CLIENT_ID, process.env.CLIENT_SECRET);

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

let conversationId = "";
let serviceUrl = "";

// Store for meeting context
const store = {
  setItem: (key, value) => storage.set(key, value),
  getItem: (key) => storage.get(key),
  deleteItem: (key) => storage.delete(key)
};

const createAgendaCard = (agenda) => {
  const card = {
    type: "AdaptiveCard",
    body: [
      {
        type: "TextBlock",
        text: "**Here is the Agenda for Today**",
        weight: "Bolder"
      }
    ],
    $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
    version: "1.4"
  };

  agenda.forEach(element => {
    card.body.push({
      type: "TextBlock",
      text: `- ${element}`,
      wrap: true
    });
  });

  return card;
};

app.on("message", async (context) => {
  const activity = context.activity;

  conversationId = activity.conversation.id;
  serviceUrl = activity.serviceUrl;
  store.setItem("conversationId", conversationId);
  store.setItem("serviceUrl", serviceUrl);

  await context.send("Welcome to SidePanel Application!");
});

app.on("install.add", async (context) => {
  conversationId = context.activity.conversation.id;
  serviceUrl = context.activity.serviceUrl;
  store.setItem("conversationId", conversationId);
  store.setItem("serviceUrl", serviceUrl);
});


// Setup API routes for the side panel
const setupApiRoutes = () => {
  const expressApp = app.http.express;

  if (expressApp) {
    expressApp.use(cors());
    expressApp.use(express.json());
    expressApp.use(express.urlencoded({ extended: true }));

    expressApp.post('/api/sendAgenda', async (req, res) => {
      try {
        const currentConversationId = store.getItem("conversationId");
        const currentServiceUrl = store.getItem("serviceUrl");
        
        if (!currentConversationId) {
          res.status(202).send({ success: false, message: 'Conversation not initialized. Send a message to the bot first.' });
          return;
        }

        const agendaArray = Array.isArray(req.body) ? req.body : String(req.body).split(",");

        const adaptiveCard = createAgendaCard(agendaArray);
        
        try {
          const token = await credentials.getToken();
          
          const response = await fetch(`${currentServiceUrl}/v3/conversations/${currentConversationId}/activities`, {
            method: 'POST',
            headers: {
              'Authorization': `Bearer ${token}`,
              'Content-Type': 'application/json'
            },
            body: JSON.stringify({
              type: 'message',
              from: { id: process.env.CLIENT_ID },
              attachments: [{
                contentType: 'application/vnd.microsoft.card.adaptive',
                content: adaptiveCard
              }]
            })
          });
          
          if (response.ok) {
            res.status(200).send({ success: true });
          } else {
            const errorText = await response.text();
            res.status(500).send({ error: 'Failed to send agenda card', details: errorText });
          }
        } catch (sendError) {
          res.status(500).send({ error: 'Failed to send agenda card' });
        }

      } catch (error) {
        res.status(500).send({ error: error.message });
      }
    });

    expressApp.post('/api/setContext', async (req, res) => {
      const meetingId = req.body.meetingId;
      const userId = req.body.userId;
      const tenantId = req.body.tenantId;
        try {
          const token = await credentials.getToken();
          const currentServiceUrl = serviceUrl || store.getItem("serviceUrl");

          const getRoleRequest = await fetch(`${currentServiceUrl}/v1/meetings/${meetingId}/participants/${userId}?tenantId=${tenantId}`,
            {
              method: 'GET',
              headers: {
                'Authorization': 'Bearer ' + token
              }
            });

          const response = await getRoleRequest.json();
          const role = response.meeting?.role;

        if (role === 'Organizer') {
          res.status(200).send(true);
        } else {
          res.status(200).send(false);
        }
      } catch (error) {
        res.status(500).send({ error: error.message });
      }
    });
  }
};

setupApiRoutes();

module.exports = app;
