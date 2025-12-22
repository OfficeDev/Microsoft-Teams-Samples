const path = require('path');
const express = require('express');
const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const { Card, TextBlock } = require("@microsoft/teams.cards");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const schedule = require('node-schedule');
const { TaskModuleResponseFactory } = require("./models/taskModuleResponseFactory");

// Create storage for conversation history and task details
const storage = new LocalStorage();
const taskDetails = {};
let conversationIds = {};

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
  config.botType === "UserAssignedMsi" ? { ...tokenCredentials } : undefined;

// Create the app with storage
const teamsApp = new App({
  ...credentialOptions,
  storage,
});

// Store reference to app for proactive messaging
app = teamsApp;

// Add general activity logger to see what events we're receiving
// Removed debug logging for cleaner production code

// Add basic message handler for testing connectivity (can be removed if you don't want echo)
teamsApp.on("message", async ({ send, activity }) => {
  // Don't echo if you don't want echo functionality
  // await send(`Echo: ${activity.text}`);
});

// Add conversation update handler to catch all conversation events AND send welcome message
teamsApp.on("conversationUpdate", async ({ send, activity }) => {
  // Handle members added in conversationUpdate (this is the main event in Teams)
  if (activity.membersAdded && activity.membersAdded.length > 0) {
    const membersAdded = activity.membersAdded;
    
    for (let member = 0; member < membersAdded.length; member++) {
      if (membersAdded[member].id !== activity.recipient.id) {
        try {
          await send("Hello and welcome! With this sample you can schedule a message reminder by selecting `...` over the message then select more action and then create-reminder and you will get reminder of the message at scheduled date and time.");
        } catch (error) {
          // Silently handle errors
        }
      }
    }
  }
});

// Base URL for task module - use same port as bot (3978) with unified server
let baseUrl;
if (process.env.BOT_ENDPOINT) {
    // Use the existing dev tunnel endpoint (port 3978)
    baseUrl = process.env.BOT_ENDPOINT;
} else if (process.env.BOT_DOMAIN) {
    // Use the existing dev tunnel domain (port 3978)
    baseUrl = `https://${process.env.BOT_DOMAIN}`;
} else {
    // Development - use localhost on same port as bot
    baseUrl = 'http://localhost:3978';
}

// Function to save task details (exact same as nodejs implementation)
const saveTaskDetails = (taskDetail) => {
  taskDetails["taskDetails"] = taskDetail;
};

// Function to create adaptive card for task module (exact same as nodejs)
const adaptiveCardForTaskModule = () => ({
  $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
  body: [
    {
      type: "TextBlock",
      size: "Default",
      weight: "Bolder",
      text: "Please click on schedule to schedule task"
    },
    {
      type: "ActionSet",
      actions: [
        {
          type: "Action.Submit",
          title: "Schedule task",
          data: {
            msteams: {
              type: "task/fetch"
            },
            id: "schedule"
          }
        }
      ]
    }
  ],
  type: "AdaptiveCard",
  version: "1.2"
});

// conversationUpdate handler is already defined above - removed duplicate

// Handle messaging extension fetch task (exact same logic as nodejs handleTeamsMessagingExtensionFetchTask)
teamsApp.on("message.ext.open", async ({ activity }) => {
  const action = activity.value;
  let title = "";
  let description = "";

  if (action.messagePayload.subject != null) {
    title = action.messagePayload.body.content;
    description = action.messagePayload.subject;
  } else {
    title = action.messagePayload.body.content;
  }

  return {
    task: {
      type: 'continue',
      value: {
        width: 350,
        height: 350,
        title: 'Schedule task',
        url: baseUrl + "/scheduleTask?title=" + title + "&description=" + description
      }
    }
  };
});

// Handle messaging extension submit action (exact same logic as nodejs handleTeamsMessagingExtensionSubmitAction)
teamsApp.on("message.ext.submit", async ({ send, activity }) => {
  const action = activity.value;
  
  // Create new object to save task details (exact same structure as nodejs)
  let taskDetail = {
    title: action.data.title,
    dateTime: action.data.dateTime,
    description: action.data.description,
  };

  saveTaskDetails(taskDetail);
  await send("Task submitted successfully. You will get reminder for the task at scheduled time");

  const currentUser = activity.from.id;
  
  // Store conversation ID for Teams AI v2 proactive messaging
  conversationIds[currentUser] = activity.conversation.id;

  // Schedule logic exactly the same as nodejs
  var dateLocal = new Date(action.data.dateTime);
  var dateLocalString = dateLocal.toLocaleString();
  var month = dateLocalString.substring(0, 2);
  var day = dateLocalString.substring(3, 5);
  var year = dateLocalString.substring(6, 10);
  var hour = dateLocal.getHours();
  var min = dateLocal.getMinutes();
  const scheduleDate = new Date(year, month - 1, day, hour, min);

  const job = schedule.scheduleJob(scheduleDate, async function () {
    // Create adaptive card exactly the same as nodejs
    const userCard = {
      $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
      body: [
        {
          type: "TextBlock",
          size: "Default",
          weight: "Bolder",
          text: "Reminder for scheduled task!"
        },
        {
          type: "TextBlock",
          size: "Default",
          weight: "Default",
          text: "Task title: " + taskDetails["taskDetails"].title,
          wrap: true
        },
        {
          type: "TextBlock",
          size: "Default",
          weight: "Default",
          text: "Task description: " + taskDetails["taskDetails"].description,
          wrap: true
        },
      ],
      type: "AdaptiveCard",
      version: "1.2"
    };

    // Send proactive message using Teams AI v2 app.send (equivalent to adapter.continueConversationAsync)
    try {
      if (conversationIds[currentUser]) {
        // Teams AI v2 expects adaptive cards directly, not wrapped in attachments array
        await teamsApp.send(conversationIds[currentUser], userCard);
      }
    } catch (error) {
      // Silently handle proactive message errors
    }
    
    // Cancel the job after sending the message to ensure it only runs once
    job.cancel();
  });

  return null;
});

module.exports = teamsApp;
