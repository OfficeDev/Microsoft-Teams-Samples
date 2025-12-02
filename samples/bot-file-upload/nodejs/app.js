const fs = require('fs');
const path = require('path');
const axios = require('axios');
const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const { generateFileName, getFileSize, writeFile } = require('./services/fileService');

const FILES_DIR = 'files';

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
  config.MicrosoftAppType === "UserAssignedMsi" ? { ...tokenCredentials } : {
    MicrosoftAppId: config.MicrosoftAppId,
    MicrosoftAppPassword: config.MicrosoftAppPassword,
    MicrosoftAppType: config.MicrosoftAppType
  };

// Filter out undefined values to prevent Teams SDK toLowerCase errors
const filteredCredentials = {};
for (const [key, value] of Object.entries(credentialOptions)) {
  if (value !== undefined && value !== null && value !== "" && 
      !(typeof value === 'string' && value === 'undefined')) {
    filteredCredentials[key] = value;
  }
}

// For MultiTenant, we don't need the token factory at all
if (config.MicrosoftAppType !== "UserAssignedMsi") {
  delete filteredCredentials.token;
}

// Create the app with storage
const app = new App({
  ...filteredCredentials,
  storage,
});

const getConversationState = (conversationId) => {
  let state = storage.get(conversationId);
  if (!state) {
    state = { count: 0 };
    storage.set(conversationId, state);
  }
  return state;
};

/**
 * Handle membersAdded event to send welcome message
 */
app.on("install.add", async (context) => {
  const activity = context.activity;

  // Check if membersAdded exists and is an array before iterating
  if (activity.membersAdded && Array.isArray(activity.membersAdded)) {
    // Iterate over all new members added to the conversation
    for (const member of activity.membersAdded) {
      if (member.id !== activity.recipient.id) {
        await context.send({
          type: 'message',
          text: 'Welcome to the File Upload Bot! You can send me files and I can also send files to you. Try saying "send file" to get a file from me.'
        });
      }
    }
  }
});

/**
 * Handle message events
 */
app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity) || "";
  const attachment = activity.attachments && activity.attachments[0];

  if (text === "/reset") {
    storage.delete(activity.conversation.id);
    await context.send("Ok I've deleted the current conversation state.");
    return;
  }

  if (text === "/count") {
    const state = getConversationState(activity.conversation.id);
    await context.send(`The count is ${state.count}`);
    return;
  }

  if (text === "/diag") {
    await context.send(JSON.stringify(activity));
    return;
  }

  if (text === "/state") {
    const state = getConversationState(activity.conversation.id);
    await context.send(JSON.stringify(state));
    return;
  }

  if (text === "/runtime") {
    const runtime = {
      nodeversion: process.version,
      sdkversion: "2.0.0", // Teams AI v2
    };
    await context.send(JSON.stringify(runtime));
    return;
  }

  // Handle file-related commands
  if (text.toLowerCase().includes('send file') || text.toLowerCase().includes('file')) {
    await sendFileCard(context);
    return;
  }



  // Handle attachments
  if (attachment) {
    const imageRegex = /image\/.*/;

    if (attachment.contentType === 'application/vnd.microsoft.teams.file.download.info') {
      // Handle file download info
      await handleFileDownload(attachment, context);
    } else if (imageRegex.test(attachment.contentType)) {
      // Handle inline image
      await processInlineImage(context);
    } else {
      // Send a default file card if no relevant attachment is found
      await sendFileCard(context);
    }
    return;
  }

  // Default echo behavior
  const state = getConversationState(activity.conversation.id);
  state.count++;
  await context.send(`[${state.count}] you said: ${text}`);
});

/**
 * Handle all invoke activities including file consent
 */
app.on("invoke", async (context) => {
  const activity = context.activity;
  
  console.log(`[INVOKE] Received invoke - Name: ${activity.name}, Type: ${activity.type}`);
  console.log(`[INVOKE] Activity value:`, JSON.stringify(activity.value, null, 2));
  
  // Handle file consent activities
  if (activity.name === 'fileConsent/invoke') {
    const fileConsentCardResponse = activity.value;
    
    console.log(`[FILE_CONSENT] Processing action: ${fileConsentCardResponse.action}`);
    
    if (fileConsentCardResponse.action === 'accept') {
      try {
        const fname = path.join(FILES_DIR, fileConsentCardResponse.context.filename);
        const fileInfo = fs.statSync(fname);
        const fileContent = fs.createReadStream(fname);

        console.log(`[FILE_CONSENT] Uploading file: ${fname}, size: ${fileInfo.size} bytes`);

        await axios.put(
          fileConsentCardResponse.uploadInfo.uploadUrl,
          fileContent, {
            headers: {
              'Content-Type': 'image/png',
              'Content-Length': fileInfo.size,
              'Content-Range': `bytes 0-${fileInfo.size - 1}/${fileInfo.size}`
            }
          });

        console.log(`[FILE_CONSENT] File uploaded successfully`);
        await fileUploadCompleted(context, fileConsentCardResponse);
        
      } catch (e) {
        console.error(`[FILE_CONSENT] Upload error: ${e.message}`);
        await fileUploadFailed(context, e.message);
      }
    } else if (fileConsentCardResponse.action === 'decline') {
      console.log(`[FILE_CONSENT] File declined by user`);
      await context.send(`The file <b>${fileConsentCardResponse.context.filename}</b> has been declined and will not be uploaded.`);
    }
    
    // Don't return anything - let Teams AI SDK handle the response
    return;
  }
  
  console.log(`[INVOKE] Unhandled invoke activity: ${activity.name}`);
});

/**
 * Handles file download and saves the file.
 * @param {object} file - The file attachment.
 * @param {object} context - The bot context.
 */
async function handleFileDownload(file, context) {
  try {
    const config = {
      responseType: 'stream'
    };
    const filePath = path.join(FILES_DIR, file.name);
    await writeFile(file.content.downloadUrl, config, filePath);

    await context.send(`<b>${file.name}</b> received and saved.`);
  } catch (error) {
    console.error('Error downloading file:', error);
    await context.send(`Error downloading file: ${error.message}`);
  }
}

/**
 * Sends a file card for user consent.
 * @param {object} context - The bot context.
 */
async function sendFileCard(context) {
  try {
    const filename = 'teams-logo.png';
    const filePath = path.join(FILES_DIR, filename);
    
    // Check if file exists
    if (!fs.existsSync(filePath)) {
      await context.send('Sorry, the sample file is not available. Please send me a file instead!');
      return;
    }
    
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
  } catch (error) {
    console.error('Error sending file card:', error);
    await context.send(`Error sending file card: ${error.message}`);
  }
}

/**
 * Notifies the user when the file upload is completed.
 * @param {object} context - The bot context.
 * @param {object} fileConsentCardResponse - The consent response.
 */
async function fileUploadCompleted(context, fileConsentCardResponse) {
  const downloadCard = {
    uniqueId: fileConsentCardResponse.uploadInfo.uniqueId,
    fileType: fileConsentCardResponse.uploadInfo.fileType
  };

  await context.send({
    type: 'message',
    text: `<b>Your file ${fileConsentCardResponse.uploadInfo.name}</b> has been successfully uploaded and is ready to download.`,
    attachments: [{
      content: downloadCard,
      contentType: 'application/vnd.microsoft.teams.card.file.info',
      name: fileConsentCardResponse.uploadInfo.name,
      contentUrl: fileConsentCardResponse.uploadInfo.contentUrl
    }]
  });
}

/**
 * Handles failed file upload and notifies the user.
 * @param {object} context - The bot context.
 * @param {string} error - The error message.
 */
async function fileUploadFailed(context, error) {
  await context.send(`<b>File upload failed.</b> Error: <pre>${error}</pre>`);
}

/**
 * Processes an inline image by saving it and notifying the user.
 * @param {object} context - The bot context.
 */
async function processInlineImage(context) {
  try {
    const file = context.activity.attachments[0];
    
    // For Teams AI SDK, we'll try without specific auth headers first
    // as the SDK should handle authentication automatically
    const config = {
      responseType: 'stream'
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
  } catch (error) {
    console.error('Error processing inline image:', error);
    await context.send(`Error processing image: ${error.message}`);
  }
}

/**
 * Creates an inline attachment for the image.
 * @param {string} fileName - The name of the file.
 * @returns {object} The inline attachment object.
 */
function getInlineAttachment(fileName) {
  const imageData = fs.readFileSync(path.join(FILES_DIR, fileName));
  const base64Image = Buffer.from(imageData).toString('base64');

  return {
    name: fileName,
    contentType: 'image/png',
    contentUrl: `data:image/png;base64,${base64Image}`
  };
}

module.exports = app;
