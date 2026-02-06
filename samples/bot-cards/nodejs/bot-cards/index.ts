// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as fs from 'fs';
import * as path from 'path';
import axios from 'axios';
import { stripMentionsText } from "@microsoft/teams.api";
import { App } from "@microsoft/teams.apps";
import { 
  AdaptiveCard, 
  TextBlock, 
  TextInput, 
  SubmitAction, 
  OpenUrlAction, 
  ShowCardAction, 
  ToggleVisibilityAction
} from "@microsoft/teams.cards";

const FILES_DIR = 'files';
const app = new App();

// Handle bot installation and new members
app.on('conversationUpdate', async (context) => {
    const { activity } = context;
    const membersAdded = (activity as any).membersAdded || [];

    for (const member of membersAdded) {
        // Check if bot was added to the conversation
        if (member.id === activity.recipient.id) {
            await sendWelcomeMessage(context);
        }
    }
});

// Handle message events
app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity) || "";
  const attachment = activity.attachments && activity.attachments[0];
  
  // Handle data submission from adaptive cards
  if (activity.value) {
    await context.send(`Data Submitted: ${JSON.stringify(activity.value)}`);
  } else if (text) {
    const normalizedText = text.trim().toLowerCase();
    
    // Handle card-related commands
    if (normalizedText.includes('card actions')) {
      await sendAdaptiveCardActions(context);
    } else if (normalizedText.includes('suggested actions')) {
      await sendSuggestedActions(context);
    } else if (normalizedText.includes('togglevisibility')) {
      await sendToggleVisibilityCard(context);
    } 
    // Handle color input
    else if (normalizedText.includes('red')) {
      await context.send('Red is the best color, I agree.');
    } else if (normalizedText.includes('blue')) {
      await context.send('Blue is the best color, I agree.');
    } else if (normalizedText.includes('yellow')) {
      await context.send('Yellow is the best color, I agree.');
    } 
    // Handle file commands
    else if (normalizedText.includes('send file') || normalizedText.includes('file')) {
      await sendFileCard(context);
    } else {
      // Unrecognized command
      await sendWelcomeMessage(context);
    }
  } else if (attachment) {
    // Handle file attachments
    const imageRegex = /image\/.*/;
    if (attachment.contentType === 'application/vnd.microsoft.teams.file.download.info') {
      await handleFileDownload(attachment, context);
    } else if (imageRegex.test(attachment.contentType)) {
      await processInlineImage(context);
    } else {
      await sendFileCard(context);
    }
  } else {
    // No activity value, text, or attachment
    await sendWelcomeMessage(context);
  }
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

// Sends welcome message with available commands
async function sendWelcomeMessage(context: any) {
  await context.send("Welcome to the Teams Bot Cards!");
}

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

// Send Adaptive Card with various actions
async function sendAdaptiveCardActions(context: any) {
  // Build innermost nested card
  const nestedCard = new AdaptiveCard(
    new TextBlock('Welcome To New Card')
  ).withActions(
    new SubmitAction({
      title: 'Click Me',
      data: { value: 'Button has Clicked' }
    })
  );

  // Build middle card
  const showCard = new AdaptiveCard(
    new TextBlock("This card's action will show another card")
  ).withActions(
    new ShowCardAction({
      title: 'Action.ShowCard',
      card: nestedCard
    })
  );

  // Build submit card
  const submitCard = new AdaptiveCard(
    new TextInput({
      id: 'name',
      label: 'Please enter your name:',
      isRequired: true,
      errorMessage: 'Name is required'
    })
  ).withActions(
    new SubmitAction({ title: 'Submit' })
  );

  // Build main card
  const card = new AdaptiveCard(
    new TextBlock('Adaptive Card Actions')
  ).withActions(
    new OpenUrlAction('https://adaptivecards.io', {
      title: 'Action Open URL'
    }),
    new ShowCardAction({
      title: 'Action Submit',
      card: submitCard
    }),
    new ShowCardAction({
      title: 'Action ShowCard',
      card: showCard
    })
  );
  
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.adaptive',
      content: JSON.parse(JSON.stringify(card))
    }]
  });
}

// Send Toggle Visibility Card
async function sendToggleVisibilityCard(context: any) {
  const card = new AdaptiveCard(
    new TextBlock('**Action.ToggleVisibility example**: click the button to show or hide a welcome message', {
      wrap: true
    }),
    new TextBlock('**Hello World!**', {
      id: 'helloWorld',
      isVisible: false,
      size: 'ExtraLarge'
    })
  ).withActions(
    new ToggleVisibilityAction({
      title: 'Click me!',
      targetElements: ['helloWorld']
    })
  );
  
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.adaptive',
      content: JSON.parse(JSON.stringify(card))
    }]
  });
}

// Send Suggested Actions with buttons
async function sendSuggestedActions(context: any) {
  const message = {
    type: 'message',
    text: 'What is your favorite color?',
    suggestedActions: {
      actions: [
        {
          type: 'imBack',
          title: 'Red',
          value: 'Red'
        },
        {
          type: 'imBack',
          title: 'Yellow',
          value: 'Yellow'
        },
        {
          type: 'imBack',
          title: 'Blue',
          value: 'Blue'
        }
      ]
    }
  };
  await context.send(message);
}

app.start().catch(console.error);