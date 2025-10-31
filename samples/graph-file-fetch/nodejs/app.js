const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const { ConsoleLogger } = require("@microsoft/teams.common/logging");
const { Client } = require("@microsoft/microsoft-graph-client");

// Create storage for conversation history
const storage = new LocalStorage();
let accessToken = null;
// Create the app with storage and OAuth configuration
const app = new App({
  storage,
  oauth: {
    defaultConnectionName: process.env.CONNECTION_NAME,
  },
  logger: new ConsoleLogger("graph-file-fetch", { level: "debug" }),
});

app.on("install.add", async ({ send }) => {
  await send("Thanks for installing the bot! Please type **/signin** to sign in to use the app functionalities.");
});

// Handle sign out command
app.message("/signout", async ({ send, signout, isSignedIn }) => {
  if (!isSignedIn) {
    await send("You are not signed in.");
    return;
  }
  await signout();
  await send("You have been signed out!");
});

app.message('/signin', async ({ send, signin, isSignedIn }) => {
  if (isSignedIn) {
    send('you are already signed in!');
  } else {
    await signin();
  }
});

// Handle successful sign-in event
app.event('signin', async ({ send, token }) => {
  accessToken = token.token;
  console.log('Sign-in successful.');
});

app.message('/signout', async ({ send, signout, isSignedIn }) => {
  if (!isSignedIn) {
    await send('you are not signed in! please type **/signin** to sign in.');
    return;
  }
  await signout(); // call signout for your auth connection...
  await send('you have been signed out!');
});

// Handle messages with attachments
app.on('message', async ({ send, activity, isSignedIn }) => {
  // Skip if this is a command
  if (activity.text && activity.text.startsWith('/')) {
    return;
  }

  const conversationType = activity.conversation?.conversationType;

  // Handle non-personal conversations (group chats and channels)
  if (conversationType !== "personal" && isSignedIn) {
    // Check if we have a valid access token
    if (!accessToken) {
      console.error('Access token is undefined. User may need to re-authenticate.');
      await send('Your session has expired. Please sign out **/signout** and sign in again using **/signin** command.');
      return;
    }
    
    console.log('Access token available:', !!accessToken);
    
    if (activity.type === 'message') {
      const messageId = activity.id;
      const graphClient = getAuthenticatedClient(accessToken);
      let attachmentUrl = null;

      try {
        if (conversationType === "groupChat") {
          const chatId = activity.conversation.id;
          attachmentUrl = await getGroupChatAttachment(graphClient, chatId, messageId);
        }
        else if (conversationType === "channel") {
          const channelData = activity.channelData;
          const channelId = channelData?.channel?.id;
          const teamId = channelData?.team?.id;
          
          if (teamId && channelId) {
            attachmentUrl = await getTeamsChannelAttachment(graphClient, teamId, channelId, messageId);
          }
        }

        if (attachmentUrl) {
          // Create an Adaptive Card with a download button
          const adaptiveCard = {
            type: 'AdaptiveCard',
            version: '1.4',
            body: [
              {
                type: 'TextBlock',
                text: 'Download File',
                weight: 'Bolder',
                size: 'Medium'
              }
            ],
            actions: [
              {
                type: 'Action.OpenUrl',
                title: 'Download',
                url: attachmentUrl
              }
            ]
          };

          await send({
            type: 'message',
            attachments: [
              {
                contentType: 'application/vnd.microsoft.card.adaptive',
                content: adaptiveCard
              }
            ]
          });
        } else {
          await send('No attachments found in the message.');
        }
      } catch (error) {
        await send(`Error accessing Graph API: ${error.message}`);
        console.error('Error processing attachment:', error);
      }
    }
  }
});

// Helper function to create authenticated Graph client
function getAuthenticatedClient(accessToken) {
  if (!accessToken) {
    throw new Error('Access token is required');
  }
  
  const client = Client.init({
    authProvider: (done) => {
      done(null, accessToken);
    }
  });
  return client;
}

// Helper function to get attachment from group chat
async function getGroupChatAttachment(graphClient, chatId, messageId) {
  try {
    // Get message with attachments from the chat
    const message = await graphClient
      .api(`/chats/${chatId}/messages/${messageId}`)
      .get();

    if (message.attachments && message.attachments.length > 0) {
      const attachment = message.attachments[0];
      return attachment.contentUrl;
    }

    return null;
  } catch (error) {
    console.error(`Error getting group chat attachment: ${error.message}`);
    throw error;
  }
}

// Helper function to get attachment from Teams channel
async function getTeamsChannelAttachment(graphClient, teamId, channelId, messageId) {
  try {
    // Get message with attachments from the team channel
    const message = await graphClient
      .api(`/teams/${teamId}/channels/${channelId}/messages/${messageId}`)
      .get();

    if (message.attachments && message.attachments.length > 0) {
      const attachment = message.attachments[0];
      return attachment.contentUrl;
    }

    return null;
  } catch (error) {
    console.error(`Error getting team channel attachment: ${error.message}`);
    throw error;
  }
}

module.exports = app;
