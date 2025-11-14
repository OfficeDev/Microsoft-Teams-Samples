// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const { contentBubbleTitles } = require('./models/contentbubbleTitle');
const AdaptiveCard = require('./resources/adaptiveCard.json');
const templateJson = require('./resources/QuestionTemplate.json');
const notificationCardJson = require('./resources/SendTargetNotificationCard.json');
const axios = require('axios');
const ACData = require("adaptivecards-templating");
const path = require('path');
const express = require('express');
const bodyParser = require('body-parser');

// Get meeting members using Bot Framework API
const getMeetingMembers = async (context, meetingId, tenantId) => {
  const serviceUrl = context.activity.serviceUrl;
  const conversationId = context.activity.conversation.id;
  const appId = process.env.CLIENT_ID || process.env.BOT_ID;
  const appPassword = process.env.CLIENT_SECRET;
  
  try {
    // Get bot token for API call
    const tokenResponse = await axios.post(
      'https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token',
      `grant_type=client_credentials&client_id=${appId}&client_secret=${appPassword}&scope=https://api.botframework.com/.default`,
      { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }
    );
    
    const botToken = tokenResponse.data.access_token;
    
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
};

// Create Express app
const server = express();

// Configure view engine for EJS
server.set("view engine", "ejs");
server.set("views", path.join(__dirname, "views"));
server.engine('html', require('ejs').renderFile);
server.use(bodyParser.urlencoded({ extended: false }));
server.use(express.json());

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

// Create the app with storage and server
const app = new App({
  ...credentialOptions,
  storage,
  server,
});

// Setup custom routes on the Teams AI Express app
setupCustomRoutes();

function setupCustomRoutes() {
  const expressApp = app.http.express;

  if (expressApp) {
    // Configure EJS view engine on the Teams AI Express app
    expressApp.set('view engine', 'ejs');
    expressApp.set('views', path.join(__dirname, 'views'));
    expressApp.engine('html', require('ejs').renderFile);

    // Add content bubble page route
    expressApp.get('/', (req, res) => {
      res.render('index.html', { 
        question: contentBubbleTitles.contentQuestion,
        appId: process.env.CLIENT_ID || process.env.MicrosoftAppId || ''
      });
    });

    expressApp.get('/test', (req, res) => {
      res.send('Test route works!');
    });
  }
}

// Helper function to create adaptive card for agenda
const createAdaptiveCard = () => {
  return {
    contentType: "application/vnd.microsoft.card.adaptive",
    content: AdaptiveCard
  };
};

// Helper function to create question adaptive card with templating
const createQuestionAdaptiveCard = (myText) => {
  const template = new ACData.Template(templateJson);
  const cardPayload = template.expand({
    $root: {
      name: myText
    }
  });
  
  return {
    contentType: "application/vnd.microsoft.card.adaptive",
    content: cardPayload
  };
};

// Helper function to create members adaptive card
const createMembersAdaptiveCard = (members) => {
  const template = new ACData.Template(notificationCardJson);
  const cardPayload = template.expand({
    $root: {
      members: members
    }
  });
  
  return {
    contentType: "application/vnd.microsoft.card.adaptive",
    content: cardPayload
  };
};

// Helper function to send targeted meeting notification
const targetedNotification = async (context, meetingId, selectedMembers) => {
  const serviceUrl = context.activity.serviceUrl;
  const appId = process.env.CLIENT_ID || process.env.BOT_ID;
  const appPassword = process.env.SECRET_BOT_PASSWORD || process.env.CLIENT_SECRET;
  
  // Get base URL - try multiple sources in order of preference
  let baseUrl = config.BOT_ENDPOINT;

  try {
    // Get bot token for API call
    const tokenResponse = await axios.post(
      'https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token',
      `grant_type=client_credentials&client_id=${appId}&client_secret=${appPassword}&scope=https://api.botframework.com/.default`,
      { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }
    );
    
    const botToken = tokenResponse.data.access_token;

    // Notification payload for meeting target notification API
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
                url: baseUrl
              }
            }
          }
        ]
      }
    };

    // Send notification using Teams API
    const url = `${serviceUrl}v1/meetings/${meetingId}/notification`;
    const response = await axios.post(url, notificationInformation, {
      headers: {
        'Authorization': `Bearer ${botToken}`,
        'Content-Type': 'application/json'
      }
    });

    return true;
  } catch (exception) {
    console.error('Error sending targeted notification:', {
      message: exception.message,
      status: exception.response?.status,
      statusText: exception.response?.statusText,
      data: exception.response?.data,
      stack: exception.stack
    });
    throw exception;
  }
};

// Helper function to send content bubble
const contentBubble = async (context) => {
  const appId = config.MicrosoftAppId;
  const baseUrl = config.BaseUrl || process.env.BaseUrl;
  
  const replyActivity = {
    type: "message",
    text: "**Please provide your valuable feedback**",
    channelData: {
      onBehalfOf: [
        {
          itemId: 0,
          mentionType: 'person',
          mri: context.activity.from.id,
          displayname: context.activity.from.name
        }
      ],
      notification: {
        alertInMeeting: true,
        externalResourceUrl: `https://teams.microsoft.com/l/bubble/${appId}?url=${baseUrl}&height=270&width=300&title=ContentBubbleinTeams&completionBotId=${appId}`
      }
    }
  };
  
  await context.send(replyActivity);
};

app.on("message", async (context) => {
  try {
    const activity = context.activity;
    const text = stripMentionsText(activity);

  // Handle adaptive card submissions
  if (activity.value) {
    const value = activity.value;
    
    // Handle targeted meeting notification submission
    if (value.Type === "SendTargetedMeetingNotification") {
      const adaptiveCardChoiceSet = value.Choice;
      
      if (!adaptiveCardChoiceSet) {
        await context.send("Please select at least one member to send notification.");
        return;
      }
      
      const selectedMembers = adaptiveCardChoiceSet.split(",");
      const meetingId = activity.channelData?.meeting?.id;
      
      if (!meetingId) {
        await context.send("Could not find meeting context. Please try again.");
        return;
      }
      
      try {
        await targetedNotification(context, meetingId, selectedMembers);
        await context.send(`Targeted notification sent to ${selectedMembers.length} member(s).`);
      } catch (error) {
        console.error('Failed to send targeted notification:', error);
        await context.send("Failed to send targeted notification. Please check bot permissions and try again.");
      }
      return;
    }
    
    // Handle dialog submission (from content bubble feedback form)
    if (value.myValue && value.title) {
      await context.send(`${activity.from.name}: **${value.myValue}** for '${value.title}'`);
      return;
    }
    
    // Handle content bubble submission
    if (value.action === 'inputselector') {
      contentBubbleTitles.contentQuestion = value.myReview;
      await contentBubble(context);
      const card = createQuestionAdaptiveCard(value.myReview);
      await context.send({
        type: "message",
        attachments: [card]
      });
      return;
    }
    
    // Handle other submissions (feedback responses)
    await context.send(`${activity.from.name}: **${value.myValue || value.myReview || 'unknown'}** for '${value.title || value.action || 'unknown'}'`);
    return;
  }

  // Handle text commands
  if (text.trim() === "SendTargetedNotification") {
    const meetingId = activity.channelData?.meeting?.id;
    const tenantId = activity.channelData?.tenant?.id;
    
    if (!meetingId) {
      await context.send("This command only works in a meeting context.");
      return;
    }
    
    try {
      // Get meeting members who are present in the meeting
      const members = await getMeetingMembers(context, meetingId, tenantId);
      
      if (members.length === 0) {
        await context.send("No active participants found in the meeting.");
        return;
      }
      
      const card = createMembersAdaptiveCard(members);
      await context.send({
        type: "message",
        attachments: [card]
      });
    } catch (error) {
      console.error('Error getting meeting members:', error);
      await context.send("Error retrieving meeting participants. Please try again.");
    }
    return;
  }

  if (text.trim() === "SendContentBubble") {
    const card = createAdaptiveCard();
    await context.send({
      type: "message",
      attachments: [card]
    });
    return;
  }

  // Default response
  await context.send("Please type `SendTargetedNotification` or `SendContentBubble` to send In-meeting notifications.");
  } catch (error) {
    console.error('Unhandled error in message handler:', {
      message: error.message,
      stack: error.stack,
      response: error.response?.data
    });
    throw error;
  }
});

// Handle dialog submit (for dialog interactions)
app.on("dialog.submit", async (context) => {
  const value = context.activity.value;
  const data = value?.data ?? value;
  
  try {
    if (data?.myValue) {
      await context.send(`${context.activity.from.name}: **${data.myValue}** for '${data.title}'`);
    } else {
      await context.send(`Received submission from ${context.activity.from.name}`);
    }
  } catch (error) {
    console.error('Error in task/submit handler:', {
      message: error.message,
      stack: error.stack,
      response: error.response?.data
    });
    throw error;
  }
});

module.exports = app;
