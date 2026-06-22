// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { ClientSecretCredential } from '@azure/identity';
import { App } from '@microsoft/teams.apps';
import { AdaptiveCard, TextBlock, OpenUrlAction } from '@microsoft/teams.cards';
import { Client } from '@microsoft/teams.graph';
import * as betaEndpoints from '@microsoft/teams.graph-endpoints-beta';
import * as dotenv from 'dotenv';

dotenv.config();

// Authenticate the app, needed for the transcript API
const credential = new ClientSecretCredential(
  process.env.TENANT_ID || '',
  process.env.CLIENT_ID || '',
  process.env.CLIENT_SECRET || ''
);

const graphClient = new Client({
  token: async () => {
    const tokenResponse = await credential.getToken('https://graph.microsoft.com/.default');
    return tokenResponse.token;
  },
});

const app = new App();

async function getMeetingTranscript(meetingResourceId: string, userId: string): Promise<string> {
  try {
    const transcriptsResponse = await graphClient.call(
      betaEndpoints.users.onlineMeetings.transcripts.list,
      {
        'user-id': userId,
        'onlineMeeting-id': meetingResourceId
      }
    );

    if (!transcriptsResponse.value?.length) {
      return '';
    }

    // Select the latest transcript by createdDateTime
    const latestTranscript = transcriptsResponse.value.reduce((latest, current) => {
      const latestDate = latest.createdDateTime ? new Date(latest.createdDateTime) : new Date(0);
      const currentDate = current.createdDateTime ? new Date(current.createdDateTime) : new Date(0);
      return currentDate > latestDate ? current : latest;
    }, transcriptsResponse.value[0]);

    if (!latestTranscript.id) {
      return '';
    }

    const content = await graphClient.call(
      betaEndpoints.users.onlineMeetings.transcripts.content.get,
      {
        'user-id': userId,
        'onlineMeeting-id': meetingResourceId,
        'callTranscript-id': latestTranscript.id
      },
      { requestConfig: { headers: { Accept: 'text/vtt' } } }
    );

    return content ?? '';
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
  for (const line of vtt.split(/\r?\n/)) {
    let trimmed = line.trim();
    if (!trimmed || trimmed.startsWith('WEBVTT') || trimmed.includes('-->')) {
      continue;
    }
    // Replace <v Speaker Name>text with Speaker Name: text
    trimmed = trimmed.replace(/<v ([^>]+)>/g, '$1: ');
    // Strip any remaining VTT tags like </v>, <c>, etc.
    trimmed = trimmed.replace(/<[^>]+>/g, '').trim();
    if (trimmed) {
      lines.push(trimmed);
    }
  }
  return lines.join('\n');
}

app.on('meetingEnd', async (context) => {
  const value = context.activity.value;
  const meetingId = context.activity.channelData?.meeting?.id ?? '';
  if (!meetingId) {
    console.error('meetingEnd event received without a valid meeting id in channelData.meeting.id');
    return;
  }
  let msGraphResourceId = context.activity.channelData?.meeting?.details?.msGraphResourceId;
  const meetingInfo = await context.api.meetings.getById(meetingId);

  let userId = '';
  if (meetingInfo && meetingInfo.organizer) {
    userId = meetingInfo.organizer.aadObjectId || '';
  }

  if (!msGraphResourceId && meetingInfo && meetingInfo.details) {
    msGraphResourceId = meetingInfo.details.msGraphResourceId;
  }

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
