const path = require('path');
const express = require('express');
const cors = require('cors');
const { stripMentionsText, MessageActivity } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const store = require("./server/services/store");
const { createAdaptiveCard } = require("./server/services/AdaptiveCardService");

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

// Handle messages - poll feedback and other interactions
app.on("message", async (client) => {
  const activity = client.activity;
  const text = stripMentionsText(activity);

  // Check if this is a poll feedback submission
  if (activity.value && activity.value.Type === "SubmitFeedback") {
    const userName = activity.from.name;
    const data = activity.value;
    const answer = data.Feedback;
    
    const taskInfoList = store.getItem("agendaList");
    const taskInfo = taskInfoList.find(x => x.Id === data.Choice);
    let personAnswered = taskInfo.personAnswered;
    
    if (!personAnswered) {
      const obj = {};
      obj[answer] = [userName];
      personAnswered = obj;
    } else {
      if (personAnswered[answer]) {
        personAnswered[answer].push(userName);
      } else {
        personAnswered[answer] = [userName];
      }
    }
    
    taskInfo.personAnswered = personAnswered;
    store.setItem("agendaList", taskInfoList);

    const option1Answered = personAnswered[taskInfo.option1] ? personAnswered[taskInfo.option1].length : 0;
    const option2Answered = personAnswered[taskInfo.option2] ? personAnswered[taskInfo.option2].length : 0;

    const total = option1Answered + option2Answered;
    const percentOption1 = total == 0 ? 0 : parseInt((option1Answered * 100) / total);
    const percentOption2 = total == 0 ? 0 : 100 - percentOption1;
    
    const card = createAdaptiveCard("Result.json", taskInfo, percentOption1, percentOption2);
    
    await client.send(
      new MessageActivity().addCard("adaptive", card)
    );
    return;
  }

});

// Handle dialog open (task module fetch) for displaying results
app.on("dialog.open", (client) => {
  const request = client.activity.value;
  const Id = request.data.Id;
  const baseUrl = process.env.BOT_ENDPOINT || process.env.BaseUrl || config.BaseURL;
  const taskModuleUrl = `${baseUrl}/Result?id=${Id}`; 
  return {
    status: 200,
    body: {
      task: {
        type: 'continue',
        value: {
          title: "Result",
          height: 250,
          width: 500,
          url: taskModuleUrl,
          fallbackUrl: taskModuleUrl
        }
      }
    }
  };
});

app.on("install.add", async ({ send, activity }) => {
  // Store conversationId and serviceUrl on bot installation
  store.setItem("conversationId", activity.conversation.id);
  store.setItem("serviceUrl", activity.serviceUrl);
});


// Setup custom API routes on Teams AI v2 Express instance
const setupApiRoutes = () => {
  const expressApp = app.http.express;

  if (expressApp) {
    // Configure CORS and body parsing
    expressApp.use(cors());
    expressApp.use(express.json());
    expressApp.use(express.urlencoded({ extended: true }));

    // API Routes
    expressApp.post('/api/sendAgenda', async (req, res) => {
      try {
        const data = req.body;
        store.setItem("agendaList", data.taskList);
        const conversationID = store.getItem("conversationId");

        if (!conversationID) {
          res.status(202).send({ success: false, message: 'Conversation not initialized. Send a message to the bot first.' });
          return;
        }

        const adaptiveCard = createAdaptiveCard('Poll.json', data.taskInfo);

        await app.send(conversationID, {
          type: 'message',
          attachments: [
            {
              contentType: 'application/vnd.microsoft.card.adaptive',
              content: adaptiveCard,
            },
          ],
        });

        res.status(200).send({ success: true });
      } catch (error) {
        console.error('Error in sendAgenda:', error);
        res.status(500).send({ error: error.message });
      }
    });

    expressApp.get('/api/getAgendaList', async (req, res) => {
      try {
        const agendaList = store.getItem("agendaList");
        res.send(agendaList);
      } catch (error) {
        console.error('Error in getAgendaList:', error);
        res.status(500).send({ error: error.message });
      }
    });

    expressApp.post('/api/setAgendaList', async (req, res) => {
      try {
        store.setItem("agendaList", req.body);
        res.status(200).send({ success: true });
      } catch (error) {
        console.error('Error in setAgendaList:', error);
        res.status(500).send({ error: error.message });
      }
    });
  }
};

// Setup routes after app is created
setupApiRoutes();

module.exports = app;
