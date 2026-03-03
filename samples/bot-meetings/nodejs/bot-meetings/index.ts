// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { ClientSecretCredential } from '@azure/identity';
import { App } from '@microsoft/teams.apps';
import { AdaptiveCard, TextBlock, OpenUrlAction } from '@microsoft/teams.cards';
import axios from 'axios';
import * as dotenv from 'dotenv';

dotenv.config();

// Authenticate the app, needed for the transcript API
const credential = new ClientSecretCredential(
  process.env.TENANT_ID || '',
  process.env.CLIENT_ID || '',
  process.env.CLIENT_SECRET || ''
);

async function getToken(): Promise<string> {
  const token = await credential.getToken('https://graph.microsoft.com/.default');
  return token.token;
}

const app = new App();

async function getMeetingTranscript(meetingResourceId: string, userId: string): Promise<string> {
  try {
    const token = await getToken();
    
    // Retrieve metadata for all the transcripts (using beta endpoint)
    const transcriptsResponse = await axios.get(
      `https://graph.microsoft.com/beta/users/${userId}/onlineMeetings/${meetingResourceId}/transcripts`,
      { headers: { 'Authorization': `Bearer ${token}` } }
    );

    if (!transcriptsResponse.data || !transcriptsResponse.data.value || transcriptsResponse.data.value.length === 0) {
      return '';
    }

    // Get the first transcript (current meeting's transcript)
    const transcriptId = transcriptsResponse.data.value[0].id;

    // Retrieve the transcript content
    const contentResponse = await axios.get(
      `https://graph.microsoft.com/beta/users/${userId}/onlineMeetings/${meetingResourceId}/transcripts/${transcriptId}/content?$format=text/vtt`,
      { headers: { 'Authorization': `Bearer ${token}` } }
    );

    return contentResponse.data || '';
  } catch (error) {
    console.error('Error retrieving transcript:', error);
    return '';
  }
}

app.on('meetingStart', async (context) => {
  const value = context.activity.value;

  const card = new AdaptiveCard(
    new TextBlock('The meeting has started.', { weight: 'Bolder', size: 'Large', wrap: true }),
    new TextBlock(`**Title:** ${value.Title || 'N/A'}`, { wrap: true }),
    new TextBlock(`**Start Time:** ${value.StartTime || 'N/A'}`, { wrap: true })
  ).withActions(
    new OpenUrlAction(value.JoinUrl, { title: 'Join Meeting' })
  );
  
  await context.send(card);
});

function parseVtt(vtt: string): string {
  const lines: string[] = [];
  for (const line of vtt.split('\n')) {
    const trimmed = line.trim();
    if (!trimmed || trimmed.startsWith('WEBVTT') || trimmed.includes('-->')) {
      continue;
    }
    // Replace <v Speaker Name>text with Speaker Name: text
    let processed = trimmed.replace(/<v ([^>]+)>/g, '$1: ');
    // Strip any remaining VTT tags like </v>, <c>, etc.
    processed = processed.replace(/<[^>]+>/g, '').trim();
    if (processed) {
      lines.push(processed);
    }
  }
  return lines.join('\n');
}

app.on('meetingEnd', async (context) => {
  const value = context.activity.value;
  const meetingId = context.activity.channelData?.meeting?.id;
  let msGraphResourceId = context.activity.channelData?.meeting?.details?.msGraphResourceId;
  const meetingInfo = await context.api.meetings.getById(meetingId);

  // Retrieve the user ID of the organizer for the transcript API
  let userId = '';
  if (meetingInfo && meetingInfo.organizer) {
    userId = meetingInfo.organizer.aadObjectId || '';
  }

  if (!msGraphResourceId && meetingInfo && meetingInfo.details) {
    msGraphResourceId = meetingInfo.details.msGraphResourceId;
  }

  // Wait 30 seconds for the transcript to become available, then retry up to 3 times
  await new Promise(resolve => setTimeout(resolve, 30000));

  let transcript = '';
  if (msGraphResourceId) {
    for (let attempt = 1; attempt <= 3; attempt++) {
      const vttTranscript = await getMeetingTranscript(msGraphResourceId, userId);
      if (vttTranscript) {
        transcript = parseVtt(vttTranscript);
        break;
      }
      if (attempt < 3) {
        console.log(`Transcript not ready, retrying in 10s (attempt ${attempt}/3)...`);
        await new Promise(resolve => setTimeout(resolve, 10000));
      }
    }
  }

  const transcriptBlocks = transcript
    ? transcript.split('\n').filter(line => line).map(line => new TextBlock(line, { wrap: true }))
    : [new TextBlock('Transcript not available for this meeting.', { wrap: true })];

  const card = new AdaptiveCard(
    new TextBlock('The meeting has ended.', { weight: 'Bolder', size: 'Large', wrap: true }),
    new TextBlock(`**End Time:** ${value.EndTime}`, { wrap: true }),
    new TextBlock('**Transcript:**', { weight: 'Bolder', wrap: true }),
    ...transcriptBlocks
  );

  await context.send(card);
});

app.on('meetingParticipantJoin', async (context) => {
  const meetingData = context.activity.value;
  const participant = meetingData.members[0];

  // Skip bot's own join event (no aadObjectId)
  if (!participant.user?.aadObjectId) return;

  const member = participant.user.name || 'A participant';
  const role = participant.meeting?.role || 'a participant';

  const card = new AdaptiveCard(
    new TextBlock(`${member} has joined the meeting as ${role}.`, {
      wrap: true,
      weight: 'Bolder'
    })
  );

  await context.send(card);
});

app.on('meetingParticipantLeave', async (context) => {
  const meetingData = context.activity.value;
  const participant = meetingData.members[0];

  // Skip bot's own leave event (no aadObjectId)
  if (!participant.user?.aadObjectId) return;

  const member = participant.user.name || 'A participant';

  const card = new AdaptiveCard(
    new TextBlock(`${member} has left the meeting.`, {
      wrap: true,
      weight: 'Bolder'
    })
  );

  await context.send(card);
});

const port = process.env.PORT || 3978;

app.start(port).then(() => {
  console.log(`Bot started, listening on http://localhost:${port}`);
});
