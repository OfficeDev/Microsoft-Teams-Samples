const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const GraphHelper = require('./helpers/graphHelper');

// Create storage for conversation history
const storage = new LocalStorage();

// Global transcript storage
global.transcriptsDictionary = [];

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

// Activity handler for message event
app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity);
  const replyText = `Echo: ${text}`;
  await context.send(replyText);
});

// Activity handler for task module fetch event
app.on("task/fetch", async (context) => {
  try {
    const meetingId = context.activity.value?.data?.meetingId;
    const taskModuleUrl = `${process.env.AppBaseUrl}/home?meetingId=${meetingId}`;

    return {
      task: {
        type: "continue",
        value: {
          title: "Meeting Transcript",
          height: 600,
          width: 600,
          url: taskModuleUrl,
        },
      },
    };
  } catch (ex) {
    console.error("Error in task/fetch handler:", ex.message);
    return {
      task: {
        type: "continue",
        value: {
          title: "Meeting Transcript",
          height: 600,
          width: 600,
          url: `${process.env.AppBaseUrl}/home`,
        },
      },
    };
  }
});

// Activity handler for ALL events
app.on("event", async (context) => {
  // Check if this is a meeting end event
  if (context.activity.name === "application/vnd.microsoft.meetingEnd") {
    try {
      // Extract meeting details from the activity value
      const meetingDetails = context.activity.value;
      const meetingId = context.activity.channelData?.meeting?.id;
      
      const graphHelper = new GraphHelper();

      // Decode the meeting ID to get the Graph resource ID
      var result = await graphHelper.GetMeetingTranscriptionsAsync(meetingId);
    
    if (result != "") {
      result = result.replace("<v", "");
      var foundIndex = transcriptsDictionary.findIndex((x) => x.id === meetingId);
      
      if (foundIndex != -1) {
        transcriptsDictionary[foundIndex].data = result;
      } else {
        transcriptsDictionary.push({
          id: meetingId,
          data: result
        });
      }

      var cardJson = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.5",
        "type": "AdaptiveCard",
        "body": [
          {
            "type": "TextBlock",
            "text": "Here is the last transcript details of the meeting.",
            "weight": "Bolder",
            "size": "Large"
          }
        ],
        "actions": [
          {
            "type": "Action.Submit",
            "title": "View Transcript",
            "data": {
              "msteams": {
                "type": "task/fetch"
              },
              "meetingId": meetingId
            }
          }
        ]
      };

      await context.send({ attachments: [{ contentType: "application/vnd.microsoft.card.adaptive", content: cardJson }] });
    } else {
      var notFoundCardJson = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.5",
        "type": "AdaptiveCard",
        "body": [
          {
            "type": "TextBlock",
            "text": "Transcript not found for this meeting.",
            "weight": "Bolder",
            "size": "Large"
          }
        ]
      };
      
      await context.send({ attachments: [{ contentType: "application/vnd.microsoft.card.adaptive", content: notFoundCardJson }] });
    }
    } catch (error) {
      console.error("Error in meetingEnd handler:", error.message);
    }
  }
});

module.exports = app;
