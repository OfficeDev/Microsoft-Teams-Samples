const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const ACData = require("adaptivecards-templating");
const axios = require("axios");
const path = require("path");

// Load the adaptive card template
const notificationCardJson = require("./resources/SendTargetNotificationCard.json");

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

// Setup custom routes
setupCustomRoutes();

function setupCustomRoutes() {
  const expressApp = app.http.express;

  if (expressApp) {
    const express = require('express');

    // Serve static files from pages directory
    expressApp.use(express.static(path.join(__dirname, 'pages')));
  }
}

const baseUrl = process.env.BOT_ENDPOINT;

/**
 * Get bot token for Bot Framework API calls
 */
async function getBotToken() {
  const appId = process.env.CLIENT_ID;
  const appPassword =process.env.CLIENT_SECRET;
  
  const response = await axios.post(
    'https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token',
    `grant_type=client_credentials&client_id=${appId}&client_secret=${appPassword}&scope=https://api.botframework.com/.default`,
    { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }
  );
  
  return response.data.access_token;
}

/**
 * Get meeting members using Bot Framework API
 */
async function getMeetingMembers(context, meetingId, tenantId) {
  const serviceUrl = context.activity.serviceUrl;
  const conversationId = context.activity.conversation.id;
  
  try {
    const botToken = await getBotToken();
    
    // Get conversation members
    const membersUrl = `${serviceUrl}v3/conversations/${conversationId}/members?includeDirectoryObjectDetails=true`;
    const membersResponse = await axios.get(membersUrl, {
      headers: {
        'Authorization': `Bearer ${botToken}`,
        'Content-Type': 'application/json'
      }
    });
    
    const members = [];
    const allMembers = membersResponse.data;
    
    // Get meeting participant details for each member
    for (const member of allMembers) {
      const aadObjectId = member.objectId;
      if (aadObjectId) {
        try {
          const participantUrl = `${serviceUrl}v1/meetings/${meetingId}/participants/${aadObjectId}?tenantId=${tenantId}`;
          
          const participantResponse = await axios.get(participantUrl, {
            headers: {
              'Authorization': `Bearer ${botToken}`,
              'Content-Type': 'application/json'
            }
          });
          
          const participantDetail = participantResponse.data;
          
          // Select only those members that are present in the meeting
          if (participantDetail.meeting && participantDetail.meeting.inMeeting) {
            members.push({ 
              id: participantDetail.user.id, 
              name: participantDetail.user.name 
            });
          }
        } catch (error) {
          console.error(`Error getting participant details for ${member.name}:`, error.response?.status, error.response?.data || error.message);
        }
      }
    }
    
    return members;
  } catch (error) {
    console.error('Error getting meeting members:', error.response?.data || error.message);
    throw error;
  }
}

/**
 * Creates an adaptive card with a list of in-meeting participants.
 * @param {Array} members - The list of members.
 * @returns {object} - The adaptive card.
 */
function createMembersAdaptiveCard(members) {
  const templatePayload = notificationCardJson;
  const template = new ACData.Template(templatePayload);

  const cardPayload = template.expand({
    $root: {
      members: members,
    },
  });

  return cardPayload;
}

/**
 * Sends a targeted meeting notification to the stage view.
 * @param {object} context - The context object for the current turn.
 * @param {string} meetingId - The ID of the meeting.
 * @param {Array} selectedMembers - The list of selected members.
 */
async function stageView(context, meetingId, selectedMembers) {
  const serviceUrl = context.activity.serviceUrl;
  
  const notificationInformation = {
    type: "targetedMeetingNotification",
    value: {
      recipients: selectedMembers,
      surfaces: [
        {
          surface: "meetingStage",
          contentType: "task",
          content: {
            value: {
              height: "300",
              width: "400",
              title: "Targeted meeting Notification",
              url: `${baseUrl}/hello.html`,
            },
          },
        },
      ],
    },
  };

  try {
    const botToken = await getBotToken();
    const url = `${serviceUrl}v1/meetings/${meetingId}/notification`;
    
    await axios.post(url, notificationInformation, {
      headers: {
        'Authorization': `Bearer ${botToken}`,
        'Content-Type': 'application/json'
      }
    });
  } catch (exception) {
    console.error("Error sending stage view notification:",exception.message);
  }
}

/**
 * Sends an app icon badging notification.
 * @param {object} context - The context object for the current turn.
 * @param {string} meetingId - The ID of the meeting.
 * @param {Array} selectedMembers - The list of selected members.
 */
async function visualIndicator(context, meetingId, selectedMembers) {
  const serviceUrl = context.activity.serviceUrl;
  
  const notificationInformation = {
    type: "targetedMeetingNotification",
    value: {
      recipients: selectedMembers,
      surfaces: [
        {
          surface: "meetingTabIcon",
        },
      ],
    },
  };

  try {
    const botToken = await getBotToken();
    const url = `${serviceUrl}v1/meetings/${meetingId}/notification`;
    
    await axios.post(url, notificationInformation, {
      headers: {
        'Authorization': `Bearer ${botToken}`,
        'Content-Type': 'application/json'
      }
    });
  } catch (exception) {
    console.error("Error sending app icon badging notification:", {
      message: exception.message
    });
  }
}

app.on("message", async (context) => {
  try {
    const activity = context.activity;
    const text = stripMentionsText(activity);

    const meetingId = activity.channelData?.meeting?.id;

    if (!meetingId) {
      await context.send("Meeting ID not found in the context.");
      return;
    }

  // Handle text commands
  if (activity.value == null) {
    const userText = text.trim();

    if (userText === "SendNotification") {
      const tenantId = activity.channelData?.tenant?.id;
      
      if (!tenantId) {
        await context.send("Tenant ID not found in the context.");
        return;
      }
      
      try {
        // Get meeting members who are present in the meeting using Bot Framework API
        const members = await getMeetingMembers(context, meetingId, tenantId);
        
        if (members.length > 0) {
          const card = createMembersAdaptiveCard(members);
          
          await context.send({
            type: "message",
            attachments: [
              {
                contentType: "application/vnd.microsoft.card.adaptive",
                content: card,
              },
            ],
          });
        } else {
          await context.send("No members are currently in the meeting.");
        }
      } catch (err) {
        await context.send("An error occurred while sending notifications.");
      }
    } else {
      await context.send("Please type `SendNotification` to send In-meeting notifications.");
    }
  }
  // Handle adaptive card submit actions
  else if (activity.value.Type === "StageViewNotification") {
    const adaptiveCardChoiceSet = activity.value.Choice;
    const selectedMembers = adaptiveCardChoiceSet.split(",");
    await stageView(context, meetingId, selectedMembers);
  } else if (activity.value.Type === "AppIconBadging") {
    const adaptiveCardChoiceSet = activity.value.Choice;
    const selectedMembers = adaptiveCardChoiceSet.split(",");
    await visualIndicator(context, meetingId, selectedMembers);
  }
  } catch (error) {
    await context.send("An unexpected error occurred.");
  }
});

module.exports = app;
