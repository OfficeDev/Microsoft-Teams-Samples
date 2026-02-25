// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { App } from "@microsoft/teams.apps";
import axios from 'axios';
import { randomUUID } from 'crypto';

const CONTENT_TYPE_FILE_DOWNLOAD = 'application/vnd.microsoft.teams.file.download.info';
const CONTENT_TYPE_FILE_CONSENT = 'application/vnd.microsoft.teams.card.file.consent';
const CONTENT_TYPE_FILE_INFO = 'application/vnd.microsoft.teams.card.file.info';

const app = new App();

const pendingUploads: Map<string, Buffer> = new Map();

app.on('install.add', async (context) => {
    await context.send("Welcome to the Bot Attachments sample!");
});

app.on("invoke", async (context) => {
  const activity = context.activity;
  
  if (activity.name === 'fileConsent/invoke') {
    const value = activity.value;
    const contextData = value.context || {};
    const filename = contextData.filename;
    const fileId = contextData.file_id;

    if (value.action === 'accept') {
      await context.send(`Accepted. Uploading <b>${filename}</b>...`);
      handleFileUpload(context, value.uploadInfo, fileId);
    } else if (value.action === 'decline') {
      pendingUploads.delete(fileId);
      await context.send(`Declined. We won't upload file <b>${filename}</b>.`);
    }
  }
});

app.on("message", async (context) => {
  const attachment = context.activity.attachments?.[0];

  if (attachment && attachment.contentType === CONTENT_TYPE_FILE_DOWNLOAD && attachment.name) {
    const content = await downloadAttachment(attachment.content.downloadUrl);
    const fileId = randomUUID();
    pendingUploads.set(fileId, content);
    await context.send(`Received <b>${attachment.name}</b>. Requesting permission to save to your OneDrive...`);
    await sendFileConsentCard(context, attachment.name, fileId);
    return;
  }

  await context.send("Welcome to the Bot Attachments sample!");
});

async function downloadAttachment(url: string): Promise<Buffer> {
  const response = await axios.get(url, { responseType: 'arraybuffer' });
  if (response.status === 200) {
    return Buffer.from(response.data);
  }
  throw new Error(`Download failed with status ${response.status}`);
}

async function uploadToOnedrive(url: string, content: Buffer): Promise<void> {
  const fileSize = content.length;
  const response = await axios.put(url, content, {
    headers: {
      'Content-Type': 'application/octet-stream',
      'Content-Length': fileSize.toString(),
      'Content-Range': `bytes 0-${fileSize - 1}/${fileSize}`
    },
    maxBodyLength: Infinity,
    maxContentLength: Infinity
  });
  if (![200, 201].includes(response.status)) {
    throw new Error(`Upload failed with status ${response.status}`);
  }
}

async function sendFileConsentCard(context: any, filename: string, fileId: string): Promise<void> {
  const consentContext = { filename: filename, file_id: fileId };
  const fileContent = pendingUploads.get(fileId);
  if (!fileContent) {
    throw new Error('File content not found');
  }
  
  await context.send({
    type: 'message',
    attachments: [{
      content: {
        description: 'This is the file I want to send you',
        sizeInBytes: fileContent.length,
        acceptContext: consentContext,
        declineContext: consentContext
      },
      contentType: CONTENT_TYPE_FILE_CONSENT,
      name: filename
    }]
  });
}

async function handleFileUpload(context: any, uploadInfo: any, fileId: string): Promise<void> {
  try {
    const content = pendingUploads.get(fileId);
    if (!content) {
      throw new Error('File content not found');
    }
    pendingUploads.delete(fileId);
    await uploadToOnedrive(uploadInfo.uploadUrl, content);   
    await context.send({
      type: 'message',
      text: `<b>${uploadInfo.name}</b> has been successfully uploaded.`,
      attachments: [{
        content: {
          uniqueId: uploadInfo.uniqueId,
          fileType: uploadInfo.fileType
        },
        contentType: CONTENT_TYPE_FILE_INFO,
        name: uploadInfo.name,
        contentUrl: uploadInfo.contentUrl
      }]
    });
  } catch (e: any) {
    pendingUploads.delete(fileId);
    console.log(`File upload failed: ${e}`);
  }
}

app.start().catch(console.error);
