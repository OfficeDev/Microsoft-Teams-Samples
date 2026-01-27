// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as fs from 'fs';
import * as path from 'path';
import axios from 'axios';
import { stripMentionsText } from "@microsoft/teams.api";
import { App } from "@microsoft/teams.apps";

const FILES_DIR = 'files';
const app = new App();

// Handle bot installation
app.on("installationUpdate", async (context) => {
  const activity = context.activity;
  if (activity.action === 'add') {
    await context.send('Welcome to the File Upload Bot! You can send me files and I can also send files to you. Try saying "send file" to get a file from me.');
  }
});

// Handle message events
app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity) || "";
  const attachment = activity.attachments && activity.attachments[0];
  if (text.toLowerCase().includes('send file') || text.toLowerCase().includes('file')) {
    await sendFileCard(context);
    return;
  }

  // Handle attachments
  if (attachment) {
    const imageRegex = /image\/.*/;
    if (attachment.contentType === 'application/vnd.microsoft.teams.file.download.info') {
      await handleFileDownload(attachment, context);
    } else if (imageRegex.test(attachment.contentType)) {
      await processInlineImage(context);
    } else {
      await sendFileCard(context);
    }
    return;
  }
  await context.send(`You said: ${text}. Try saying "send file" to get a file from me.`);
});

// Handle all invoke activities including file consent
app.on("invoke", async (context) => {
  const activity = context.activity;  
  if (activity.name === 'fileConsent/invoke') {
    const fileConsentCardResponse = activity.value;        
    if (fileConsentCardResponse?.action === 'accept') {
      try {
        const fname = path.join(FILES_DIR, fileConsentCardResponse.context?.filename || 'unknown');
        const fileInfo = fs.statSync(fname);
        const fileContent = fs.readFileSync(fname);
        const uploadUrl = fileConsentCardResponse.uploadInfo?.uploadUrl;
        if (!uploadUrl) {
          throw new Error('Upload URL not found');
        }
        await axios.put(uploadUrl, fileContent, {
          headers: {
            'Content-Type': 'application/octet-stream',
            'Content-Length': fileInfo.size.toString(),
            'Content-Range': `bytes 0-${fileInfo.size - 1}/${fileInfo.size}`
          },
          maxBodyLength: Infinity,
          maxContentLength: Infinity
        });
        await fileUploadCompleted(context, fileConsentCardResponse);        
      } catch (e: any) {
        console.error(`[FILE_CONSENT] Upload error: ${e.message}`);
        await fileUploadFailed(context, e.message);
      }
    } else if (fileConsentCardResponse?.action === 'decline') {
      await context.send(`The file <b>${fileConsentCardResponse.context?.filename}</b> has been declined and will not be uploaded.`);
    }
    return;
  }
});

// Handles file download and saves the file
async function handleFileDownload(file: any, context: any) {
  try {
    const config = {
      responseType: 'stream' as const
    };
    const filePath = path.join(FILES_DIR, file.name);
    await writeFile(file.content.downloadUrl, config, filePath);
    await context.send(`<b>${file.name}</b> received and saved.`);
  } catch (error: any) {
    console.error('Error downloading file:', error);
    await context.send(`Error downloading file: ${error.message}`);
  }
}

// Sends a file card for user consent
async function sendFileCard(context: any) {
  try {
    const filename = 'teams-logo.png';
    const filePath = path.join(FILES_DIR, filename);
    const stats = fs.statSync(filePath);
    const fileSize = stats.size;
    const consentContext = { filename: filename };
    const fileCard = {
      description: 'This is the file I want to send you',
      sizeInBytes: fileSize,
      acceptContext: consentContext,
      declineContext: consentContext
    };
    await context.send({
      type: 'message',
      attachments: [{
        content: fileCard,
        contentType: 'application/vnd.microsoft.teams.card.file.consent',
        name: filename
      }]
    });
  } catch (error: any) {
    console.error('Error sending file card:', error);
    await context.send(`Error sending file card: ${error.message}`);
  }
}

// Notifies the user when the file upload is completed
async function fileUploadCompleted(context: any, fileConsentCardResponse: any) {
  const downloadCard = {
    uniqueId: fileConsentCardResponse.uploadInfo?.uniqueId,
    fileType: fileConsentCardResponse.uploadInfo?.fileType
  };
  await context.send({
    type: 'message',
    text: `<b>Your file ${fileConsentCardResponse.uploadInfo?.name}</b> has been successfully uploaded and is ready to download.`,
    attachments: [{
      content: downloadCard,
      contentType: 'application/vnd.microsoft.teams.card.file.info',
      name: fileConsentCardResponse.uploadInfo?.name,
      contentUrl: fileConsentCardResponse.uploadInfo?.contentUrl
    }]
  });
}

// Handles failed file upload and notifies the user
async function fileUploadFailed(context: any, error: string) {
  await context.send(`<b>File upload failed.</b> Error: <pre>${error}</pre>`);
}

// Processes an inline image by saving it and notifying the user
async function processInlineImage(context: any) {
  try {
    const file = context.activity.attachments[0];
    const config = {
      responseType: 'stream' as const
    };
    const fileName = await generateFileName(FILES_DIR);
    const filePath = path.join(FILES_DIR, fileName);
    await writeFile(file.contentUrl, config, filePath);
    const fileSize = await getFileSize(filePath);
    const inlineAttachment = getInlineAttachment(fileName);
    await context.send({
      type: 'message',
      text: `Image <b>${fileName}</b> of size <b>${fileSize}</b> bytes received and saved.`,
      attachments: [inlineAttachment]
    });
  } catch (error: any) {
    console.error('Error processing inline image:', error);
    await context.send(`Error processing image: ${error.message}`);
  }
}

// Creates an inline attachment for the image
function getInlineAttachment(fileName: string) {
  const imageData = fs.readFileSync(path.join(FILES_DIR, fileName));
  const base64Image = Buffer.from(imageData).toString('base64');
  return {
    name: fileName,
    contentType: 'image/png',
    contentUrl: `data:image/png;base64,${base64Image}`
  };
}

// Generates a file name based on a sequence of existing files
async function generateFileName(fileDir: string): Promise<string> {
  const filenamePrefix = 'UserAttachment';
  const files = await fs.promises.readdir(fileDir);
  const filteredFiles = files
    .filter(f => f.includes(filenamePrefix))
    .map(f => parseInt(f.split(filenamePrefix)[1].split('.')[0]))
    .filter(num => !isNaN(num));
  const maxSeq = filteredFiles.length > 0 ? Math.max(...filteredFiles) : 0;
  const filename = `${filenamePrefix}${maxSeq + 1}.png`;
  return filename;
}

// Downloads content from a URL and saves it to the specified file path
async function writeFile(contentUrl: string, config: any, filePath: string): Promise<void> {
  try {
    const response = await axios({ method: 'GET', url: contentUrl, ...config });
    return new Promise((resolve, reject) => {
      response.data
        .pipe(fs.createWriteStream(filePath))
        .once('finish', resolve)
        .once('error', reject);
    });
  } catch (error: any) {
    console.error('Error downloading the file:', error.message);
    throw new Error('Failed to download file');
  }
}

// Returns the size of a file
async function getFileSize(filePath: string): Promise<number> {
  try {
    const stats = await fs.promises.stat(filePath);
    return stats.size;
  } catch (error: any) {
    console.error('Error retrieving file size:', error.message);
    throw new Error('Failed to retrieve file size');
  }
}

app.start().catch(console.error);