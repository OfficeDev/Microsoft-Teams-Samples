// @ts-nocheck
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { App } from '@microsoft/teams.apps';
import { stripMentionsText } from '@microsoft/teams.api';
import axios from 'axios';
import qs from 'qs';

// Global transcript storage
const transcriptsDictionary: { id: string; data: string }[] = [];

// Storage for meeting start times (keyed by conversationId)
const meetingStartTimes: Map<string, string> = new Map();

// Create the app (it has a built-in HttpPlugin)
const app = new App();

// Add middleware to intercept task/fetch before SDK processes it
app.http.use('/api/messages', async (req: any, res: any, next: any) => {
  if (req.body && req.body.type === 'invoke' && req.body.name === 'task/fetch') {
    try {
      const meetingId = req.body.value?.data?.meetingId;
      const appBaseUrl = process.env.APP_BASE_URL;
      const taskModuleUrl = `${appBaseUrl}/home?meetingId=${encodeURIComponent(meetingId)}`;
      
      const response = {
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
      
      res.status(200).json(response);
      return; // Don't call next() since we handled it
    } catch (error) {
      console.error("Error handling task/fetch:", error);
    }
  }
  next();
});

// Graph Helper Functions
async function GetAccessToken(): Promise<string> {
  const data = qs.stringify({
    'grant_type': 'client_credentials',
    'client_id': process.env.CLIENT_ID,
    'scope': 'https://graph.microsoft.com/.default',
    'client_secret': process.env.CLIENT_SECRET
  });

  const response = await axios.post(
    `https://login.microsoftonline.com/${process.env.TENANT_ID}/oauth2/v2.0/token`,
    data,
    { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }
  );
  return response.data.access_token;
}

async function GetBotToken(): Promise<string> {
  const data = qs.stringify({
    'grant_type': 'client_credentials',
    'client_id': process.env.CLIENT_ID,
    'scope': 'https://api.botframework.com/.default',
    'client_secret': process.env.CLIENT_SECRET
  });

  const response = await axios.post(
    `https://login.microsoftonline.com/${process.env.TENANT_ID}/oauth2/v2.0/token`,
    data,
    { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }
  );
  return response.data.access_token;
}

async function GetMeetingInfo(serviceUrl: string, meetingId: string): Promise<any> {
  try {
    const botToken = await GetBotToken();
    const meetingInfoUrl = `${serviceUrl}v1/meetings/${encodeURIComponent(meetingId)}`;
    
    const response = await axios.get(meetingInfoUrl, {
      headers: { 'Authorization': `Bearer ${botToken}` }
    });
    return response.data;
  } catch (error: any) {
    console.error("Error getting meeting info:", error.response?.data?.message || error.message);
    return null;
  }
}

async function GetMeetingTranscriptionsAsync(meetingId: string, serviceUrl?: string): Promise<string> {
  try {
    const accessToken = await GetAccessToken();
    const graphEndpoint = 'https://graph.microsoft.com/beta';
    const userId = process.env.USER_ID;
    
    if (!userId) {
      console.error("USER_ID is not configured.");
      return "";
    }
    
    let msGraphResourceId: string | null = null;
    
    if (serviceUrl) {
      const meetingInfo = await GetMeetingInfo(serviceUrl, meetingId);
      if (meetingInfo?.details?.msGraphResourceId) {
        msGraphResourceId = meetingInfo.details.msGraphResourceId;
      }
    }
    
    if (!msGraphResourceId) {
      msGraphResourceId = meetingId;
    }
    
    const getAllTranscriptsEndpoint = `${graphEndpoint}/users/${userId}/onlineMeetings/${msGraphResourceId}/transcripts`;
    
    const transcriptsResponse = await axios.get(getAllTranscriptsEndpoint, {
      headers: { 'Authorization': `Bearer ${accessToken}` }
    });
    
    const transcripts = transcriptsResponse.data.value;

    if (transcripts && transcripts.length > 0) {
      const getTranscriptEndpoint = `${getAllTranscriptsEndpoint}/${transcripts[0].id}/content?$format=text/vtt`;
      const transcript = (await axios.get(getTranscriptEndpoint, {
        headers: { 'Authorization': `Bearer ${accessToken}` }
      })).data;
      return transcript;
    }
    return "";
  } catch (ex: any) {
    console.error("Graph API Error:", ex.response?.data?.error?.message || ex.message);
    return "";
  }
}

async function SendCardToConversation(serviceUrl: string, conversationId: string, card: any, activity: any): Promise<void> {
  try {
    const token = await GetBotToken();
    await axios.post(
      `${serviceUrl}v3/conversations/${conversationId}/activities`,
      {
        type: 'message',
        from: activity.recipient,
        recipient: activity.from,
        conversation: { id: conversationId },
        attachments: [{ contentType: 'application/vnd.microsoft.card.adaptive', content: card }]
      },
      { headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' } }
    );
  } catch (error: any) {
    console.error("Error sending card:", error.message);
  }
}

// ========== Adaptive Card Builders for Meeting Events ==========

function AdaptiveCardForMeetingStart(meetingObject: any) {
  return {
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    type: 'AdaptiveCard',
    version: '1.4',
    body: [
      { type: 'TextBlock', size: 'Medium', weight: 'Bolder', text: (meetingObject.Title || 'Meeting') + ' - started' },
      {
        type: 'ColumnSet', spacing: 'medium',
        columns: [
          { type: 'Column', width: 1, items: [{ type: 'TextBlock', size: 'Medium', weight: 'Bolder', text: 'Start Time : ' }] },
          { type: 'Column', width: 3, items: [{ type: 'TextBlock', size: 'Medium', text: new Date(meetingObject.StartTime).toString() }] }
        ]
      },
      {
        type: 'ActionSet',
        actions: [{ type: 'Action.OpenUrl', title: 'Join meeting', url: meetingObject.JoinUrl }]
      }
    ]
  };
}

function AdaptiveCardForMeetingEnd(meetingObject: any, meetingDurationText: string) {
  return {
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    type: 'AdaptiveCard',
    version: '1.4',
    body: [
      { type: 'TextBlock', size: 'Medium', weight: 'Bolder', text: (meetingObject.Title || 'Meeting') + ' - ended' },
      {
        type: 'ColumnSet', spacing: 'medium',
        columns: [
          { type: 'Column', width: 1, items: [
            { type: 'TextBlock', size: 'Medium', weight: 'Bolder', text: 'End Time : ' },
            { type: 'TextBlock', size: 'Medium', weight: 'Bolder', text: 'Total Duration : ' }
          ]},
          { type: 'Column', width: 3, items: [
            { type: 'TextBlock', size: 'Medium', text: new Date(meetingObject.EndTime).toString() },
            { type: 'TextBlock', size: 'Medium', text: meetingDurationText }
          ]}
        ]
      }
    ]
  };
}

function AdaptiveCardForMeetingParticipantEvents(userName: string, action: string) {
  return {
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    type: 'AdaptiveCard',
    version: '1.4',
    body: [
      {
        type: 'RichTextBlock', spacing: 'Medium',
        inlines: [
          { type: 'TextRun', text: userName, weight: 'Bolder', size: 'Default' },
          { type: 'TextRun', text: action, weight: 'Default', size: 'Default' }
        ]
      }
    ]
  };
}

// ========== Activity Handlers ==========

// Activity handler for message event
app.on("message", async (context) => {
  const text = stripMentionsText(context.activity);
  await context.send(`Echo: ${text}`);
});

// Activity handler for task module fetch event
app.on("task/fetch", async (context) => {
  const meetingId = context.activity.value?.data?.meetingId;
  const appBaseUrl = process.env.APP_BASE_URL;
  
  return {
    task: {
      type: "continue",
      value: {
        title: "Meeting Transcript",
        height: 600,
        width: 600,
        url: `${appBaseUrl}/home?meetingId=${encodeURIComponent(meetingId)}`,
      },
    },
  };
});

// Track processed events to prevent duplicates
const processedEvents: Set<string> = new Set();

// Activity handler for all meeting events
app.on("event", async (context) => {
  const eventName = context.activity.name;
  const meetingObject = context.activity.value;
  const conversationId = context.activity.conversation?.id;
  const serviceUrl = context.activity.serviceUrl;

  // Deduplicate: create a unique key from event name + conversation + timestamp
  const eventKey = `${eventName}-${conversationId}-${context.activity.timestamp}`;
  if (processedEvents.has(eventKey)) return;
  processedEvents.add(eventKey);
  // Clean up old entries after 60 seconds
  setTimeout(() => processedEvents.delete(eventKey), 60000);

  // ---- Meeting Start Event ----
  if (eventName === "application/vnd.microsoft.meetingStart") {
    // Store start time for duration calculation
    if (conversationId && meetingObject?.StartTime) {
      meetingStartTimes.set(conversationId, meetingObject.StartTime);
    }

    const card = AdaptiveCardForMeetingStart(meetingObject);
    await SendCardToConversation(serviceUrl, conversationId, card, context.activity);
  }

  // ---- Meeting End Event ----
  if (eventName === "application/vnd.microsoft.meetingEnd") {
    const meetingId = context.activity.channelData?.meeting?.id;
    
    // Calculate meeting duration
    const startTime = meetingStartTimes.get(conversationId) || '';
    let meetingDurationText = 'N/A';
    if (startTime && meetingObject?.EndTime) {
      const timeDuration = new Date(meetingObject.EndTime).getTime() - new Date(startTime).getTime();
      const minutes = Math.floor(timeDuration / 60000);
      const seconds = ((timeDuration % 60000) / 1000).toFixed(0);
      meetingDurationText = minutes >= 1 ? minutes + "min " + seconds + "s" : seconds + "s";
      meetingStartTimes.delete(conversationId); // Clean up
    }

    // Send meeting ended card with duration info
    const endCard = AdaptiveCardForMeetingEnd(meetingObject, meetingDurationText);
    await SendCardToConversation(serviceUrl, conversationId, endCard, context.activity);
    
    // Wait for transcript to be generated, then send transcript card
    setTimeout(async () => {
      let result = "";
      
      // Retry logic: try up to 3 times
      for (let attempt = 1; attempt <= 3; attempt++) {
        result = await GetMeetingTranscriptionsAsync(meetingId, serviceUrl);
        
        if (result && result !== "") {
          break;
        }
        if (attempt < 3) {
          await new Promise(resolve => setTimeout(resolve, 10000));
        }
      }
      
      if (result && result !== "") {
        const cleanedResult = result.replace("<v", "");
        const foundIndex = transcriptsDictionary.findIndex((x) => x.id === meetingId);
        if (foundIndex !== -1) {
          transcriptsDictionary[foundIndex].data = cleanedResult;
        } else {
          transcriptsDictionary.push({ id: meetingId, data: cleanedResult });
        }
      }
      
      const cardToSend = result && result !== "" ? {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.5",
        "type": "AdaptiveCard",
        "body": [{ "type": "TextBlock", "text": "Meeting transcript is ready.", "weight": "Bolder", "size": "Large" }],
        "actions": [{ "type": "Action.Submit", "title": "View Transcript", "data": { "msteams": { "type": "task/fetch" }, "meetingId": meetingId } }]
      } : {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.5",
        "type": "AdaptiveCard",
        "body": [{ "type": "TextBlock", "text": "Transcript not found for this meeting.", "weight": "Bolder", "size": "Large" }]
      };
      
      await SendCardToConversation(serviceUrl, conversationId, cardToSend, context.activity);
    }, 30000);
  }

  // ---- Participant Join Event ----
  if (eventName === "application/vnd.microsoft.meetingParticipantJoin") {
    const members = meetingObject?.members || context.activity.value?.members;
    if (members && members.length > 0) {
      const member = members[0];
      // Skip bot's own join event (no aadObjectId)
      if (!member.user?.aadObjectId) return;
      const userName = member.user?.name || member.user?.userPrincipalName || 'A participant';
      const card = AdaptiveCardForMeetingParticipantEvents(userName, ' has joined the meeting.');
      await SendCardToConversation(serviceUrl, conversationId, card, context.activity);
    }
  }

  // ---- Participant Leave Event ----
  if (eventName === "application/vnd.microsoft.meetingParticipantLeave") {
    const members = meetingObject?.members || context.activity.value?.members;
    if (members && members.length > 0) {
      const member = members[0];
      // Skip bot's own leave event (no aadObjectId)
      if (!member.user?.aadObjectId) return;
      const userName = member.user?.name || member.user?.userPrincipalName || 'A participant';
      const card = AdaptiveCardForMeetingParticipantEvents(userName, ' left the meeting.');
      await SendCardToConversation(serviceUrl, conversationId, card, context.activity);
    }
  }
});

// Custom HTTP routes using the SDK's built-in HttpPlugin
app.http.get('/config', (req, res) => {
  const host = req.headers.host;
  res.setHeader('Content-Type', 'text/html');
  res.send(`<html><body><script src="https://res.cdn.office.net/teams-js/2.0.0/js/MicrosoftTeams.min.js"></script><script>microsoftTeams.app.initialize().then(() => { microsoftTeams.pages.config.registerOnSaveHandler((saveEvent) => { microsoftTeams.pages.config.setConfig({ entityId: "meetingTranscript", contentUrl: "https://${host}/home", websiteUrl: "https://${host}/home" }); saveEvent.notifySuccess(); }); microsoftTeams.pages.config.setValidityState(true); });</script></body></html>`);
});

app.http.get('/home', async (req, res) => {
  let transcript = "Transcript not found.";
  
  // Parse query string manually (SDK's HttpPlugin may not populate req.query)
  const url = new URL(req.url || '', `https://${req.headers.host}`);
  const meetingId = url.searchParams.get('meetingId') || '';
  
  if (meetingId) {
    const foundIndex = transcriptsDictionary.findIndex((x) => x.id === meetingId);
    
    if (foundIndex !== -1) {
      transcript = transcriptsDictionary[foundIndex].data;
    } else {
      const result = await GetMeetingTranscriptionsAsync(meetingId);
      if (result !== "") {
        transcriptsDictionary.push({ id: meetingId, data: result });
        transcript = result;
      }
    }
  }

  res.setHeader('Content-Type', 'text/html');
  res.send(`<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Meeting Transcript</title>
    <style>
        #transcription { white-space: pre-wrap; word-wrap: break-word; }
        .pre-container { width: 35rem !important; }
    </style>
</head>
<body>
    <p>Transcription details:</p>
    <div class="pre-container">
        <pre id="transcription">${transcript}</pre>
    </div>
</body>
</html>`);
});

// Start the application
const port = process.env.PORT || 3978;

app.start(port).then(() => {
  console.log(`Bot started, listening on http://localhost:${port}`);
});
