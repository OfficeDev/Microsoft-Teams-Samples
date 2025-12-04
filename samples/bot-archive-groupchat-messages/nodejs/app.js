const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const { ConsoleLogger } = require("@microsoft/teams.common/logging");
const { Client } = require('@microsoft/microsoft-graph-client');
const fs = require('fs');
const path = require('path');

// Create storage for conversation history
const storage = new LocalStorage();

// Create the app with storage and OAuth configuration
const app = new App({
  storage,
  oauth: {
    defaultConnectionName: process.env.CONNECTION_NAME || "oauthbotsetting",
  },
  logger: new ConsoleLogger("bot-archive-groupchat-messages", { level: "debug" }),
});



// Helper function to create file consent card
const createFileConsentCard = (filename, fileSize) => {
  const consentContext = { filename: filename };
  
  return {
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.teams.card.file.consent',
      name: filename,
      content: {
        description: 'This is the chat messages file I want to send you',
        sizeInBytes: fileSize,
        acceptContext: consentContext,
        declineContext: consentContext
      }
    }]
  };
};

// Helper function to create and save chat messages file
const createChatFile = (messages) => {
  const filePath = path.join(__dirname, 'chat.txt');
  const chatContent = messages.map(msg => {
    const timestamp = new Date(msg.createdDateTime).toLocaleString();
    const sender = msg.from?.user?.displayName || 'Unknown';
    const content = msg.body?.content || '';
    return `[${timestamp}] ${sender}: ${content}`;
  }).join('\n');
  
  fs.writeFileSync(filePath, chatContent);
  return filePath;
};

// Handle app installation
app.on("install.add", async ({ send }) => {
  await send("Thanks for installing the bot! Please type **login** to sign in.");
});

// Handle logout command
app.message("logout", async ({ send, signout, isSignedIn }) => {
  if (!isSignedIn) {
    await send("You are not signed in.");
    return;
  }
  await signout();
  await send("You have been signed out!");
});

// Handle sign in command
app.message('login', async ({ send, signin, isSignedIn }) => {
  if (isSignedIn) {
    await send('You are already signed in!');
  } else {
    await signin();
  }
});

// Handle successful sign-in event
app.event('signin', async ({ send, token }) => {
  await send(`Signed in successfully! Please type **getchat** to fetch chat messages or **logout** to sign out.`);
});

// Handle get chat messages command
app.message('getchat', async ({ send, userToken, isSignedIn, activity }) => {
  if (!isSignedIn) {
    await send('You are not signed in! Please type **login** to sign in.');
    return;
  }

  if (activity.conversation.conversationType === 'personal') {
    await send("This command only works in group chats. Please add the bot to a group chat and try again.");
    return;
  }

  try {
    const graphClient = Client.init({
      authProvider: (done) => {
        done(null, userToken); // userToken is already the token string
      }
    });

    // Get chat messages using beta API - exactly like the original sample
    const chatId = activity.conversation.id;
    const result = await graphClient
      .api(`/chats/${chatId}/messages`)
      .version('beta')
      .get();

    if (result?.value && result.value.length > 0) {
      // Create file with chat messages
      const filePath = createChatFile(result.value);
      const stats = fs.statSync(filePath);
      
      // Add timestamp to filename to create new upload session
      const timestamp = new Date().getTime();
      const filename = `chat_${timestamp}.txt`;
      
      // Send file consent card
      const fileCard = createFileConsentCard(filename, stats.size);
      await send(fileCard);
    } else {
      await send("No messages found in this chat.");
    }
    
  } catch (error) {
    console.error('Error fetching chat messages:', error);
    await send("Error fetching chat messages. Please make sure the bot has necessary permissions.");
  }
});

// Handle default messages
app.on('message', async ({ send, activity, isSignedIn }) => {
  if (isSignedIn) {
    await send(`You said: "${activity.text}". Please type **getchat** to fetch messages or **logout** to sign out.`);
  } else {
    await send(`You said: "${activity.text}". Please type **login** to sign in.`);
  }
});

// Handle invoke activities (including file consent)
app.on('invoke', async (context) => {
  if (context.activity.name === 'fileConsent/invoke') {
    const fileConsentCardResponse = context.activity.value;
    
    if (fileConsentCardResponse.action === 'accept') {
      try {
        const filePath = path.join(__dirname, 'chat.txt');
        const fileContent = fs.readFileSync(filePath);
        const fileSize = fileContent.length;
        
        // Upload file to SharePoint
        const response = await fetch(fileConsentCardResponse.uploadInfo.uploadUrl, {
          method: 'PUT',
          headers: {
            'Content-Type': 'text/plain',
            'Content-Length': fileSize,
            'Content-Range': `bytes 0-${fileSize - 1}/${fileSize}`
          },
          body: fileContent
        });

        if (response.ok) {
          // Send file info card - this notifies Teams that the upload is complete
          const downloadCard = {
            uniqueId: fileConsentCardResponse.uploadInfo.uniqueId,
            fileType: fileConsentCardResponse.uploadInfo.fileType
          };

          await context.send({
            type: 'message',
            text: `**File uploaded.** Your file **${fileConsentCardResponse.uploadInfo.name}** is ready to download`,
            attachments: [{
              contentType: 'application/vnd.microsoft.teams.card.file.info',
              name: fileConsentCardResponse.uploadInfo.name,
              content: downloadCard,
              contentUrl: fileConsentCardResponse.uploadInfo.contentUrl
            }]
          });
          
          // Clean up temp file
          fs.unlinkSync(filePath);
        } else {
          const errorText = await response.text();
          await context.send(`Failed to upload file: ${errorText}`);
        }
      } catch (error) {
        await context.send(`Error uploading file: ${error.message}`);
      }
    } else {
      await context.send("File upload was declined.");
    }
  }
});

module.exports = app;
