const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const { Client } = require('@microsoft/microsoft-graph-client');
const { SubscriptionManagementService } = require('./Helper/SubscriptionManager');
const { subscriptionConfiguration } = require('./constant');

// Create storage for conversation history
const storage = new LocalStorage();

// Store conversation references for proactive messaging
const conversationReferences = {};

// Create the app with storage and OAuth configuration
const app = new App({
  storage,
  oauth: {
    defaultConnectionName: process.env.CONNECTION_NAME || "oauthbotsetting",
  },
});

// Store access token for Graph operations
let globalAccessToken = null;

// Helper function to create status notification card
const createStatusCard = (title, availability, activity) => {
  return {
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.adaptive',
      content: {
        type: 'AdaptiveCard',
        version: '1.2',
        body: [
          {
            type: 'TextBlock',
            text: title,
            weight: 'Bolder',
            size: 'Medium'
          },
          {
            type: 'TextBlock',
            text: `Availability: ${availability}`,
            color: availability === 'Available' ? 'Good' : 'Warning'
          },
          {
            type: 'TextBlock',
            text: `Activity: ${activity}`,
            wrap: true
          }
        ]
      }
    }]
  };
};

// Helper function to add conversation reference
const addConversationReference = (activity) => {
  const conversationReference = {
    id: activity.conversation.id,
    user: activity.from,
    bot: activity.recipient,
    conversation: activity.conversation,
    channelId: activity.channelId,
    serviceUrl: activity.serviceUrl
  };
  
  conversationReferences[activity.conversation.id] = conversationReference;
};

// Handle app installation
app.on("install.add", async ({ send, activity }) => {
  // Store conversation reference for proactive messaging
  addConversationReference(activity);
  await send("Thanks for installing the Change Notification bot! Please type **login** to sign in and start monitoring your presence status.");
});

// Handle logout command
app.message("logout", async ({ send, signout, isSignedIn }) => {
  if (!isSignedIn) {
    await send("You are not signed in.");
    return;
  }
  
  // Clear stored token
  globalAccessToken = null;
  
  await signout();
  await send("You have been signed out!");
});

// Handle sign in command
app.message('login', async ({ send, signin, isSignedIn, activity }) => {
  // Store conversation reference
  addConversationReference(activity);
  
  if (isSignedIn) {
    await send('You are already signed in!');
  } else {
    await signin();
  }
});

// Handle successful sign-in event
app.event('signin', async ({ send, userToken, activity }) => {
  try {
    // Store the access token globally for webhook usage
    globalAccessToken = userToken;
    
    // Store conversation reference
    addConversationReference(activity);
    
    // Create subscription for user presence changes
    const subscriptionManager = new SubscriptionManagementService(userToken);
    const userId = activity.from.aadObjectId;
    
    if (userId) {
      await subscriptionManager.createSubscription(subscriptionConfiguration, userId);
      await send(`Signed in successfully! Your presence status is now being monitored. Change your status and type **status** to get notifications.`);
    } else {
      await send(`Signed in successfully, but couldn't get your Azure AD ID. Please try again.`);
    }
  } catch (error) {
    console.error('Error during signin:', error);
    await send("Signed in, but there was an error setting up presence monitoring. Please try the **login** command again.");
  }
});

// Handle status check command
app.message('status', async ({ send, userToken, isSignedIn, activity }) => {
  if (!isSignedIn) {
    await send('You are not signed in! Please type **login** to sign in.');
    return;
  }

  try {
    const userId = activity.from.aadObjectId;
    if (!userId) {
      await send("Could not get your user ID. Please try signing out and signing in again.");
      return;
    }

    const graphClient = Client.init({
      authProvider: (done) => {
        done(null, userToken);
      }
    });

    // Get current user presence
    const userState = await graphClient
      .api(`/communications/presences/${userId}`)
      .get();

    const statusCard = createStatusCard(
      "Your Current Status", 
      userState.availability, 
      userState.activity
    );
    
    await send(statusCard);
    
  } catch (error) {
    console.error('Error fetching user status:', error);
    await send("Error fetching your current status. Please make sure the bot has necessary permissions.");
  }
});

// Handle default messages
app.on('message', async ({ send, activity, isSignedIn }) => {
  // Store conversation reference for any message
  addConversationReference(activity);
  
  if (isSignedIn) {
    await send(`You said: "${activity.text}". Available commands: **status** to check current status, **logout** to sign out.`);
  } else {
    await send(`You said: "${activity.text}". Please type **login** to sign in and start monitoring your presence.`);
  }
});

// Helper function to  get user state using stored token
const getUserState = async (userStateURL) => {
  try {
    if (!globalAccessToken) {
      console.log('No access token available');
      return null;
    }

    const client = Client.init({
      authProvider: (done) => {
        done(null, globalAccessToken);
      }
    });
    
    const userState = await client.api(userStateURL).get();
    return userState;
  } catch (error) {
    console.log(`Error fetching user state: ${error.message}`);
    return null;
  }
};

// Helper function to send proactive messages
const sendProactiveMessage = async (conversationReference, message) => {
  try {
    // Use the adapter to send proactive messages
    await app.server.adapter.continueConversation(conversationReference, async (context) => {
      await context.sendActivity(message);
    });
  } catch (error) {
    console.error('Error sending proactive message:', error);
  }
};

// Webhook endpoint handler for Graph change notifications
const handleNotification = async (notificationData) => {
  try {
    console.log("Processing change notification:", notificationData);
    
    if (notificationData && notificationData.value && notificationData.value[0]) {
      const resourceData = notificationData.value[0].resourceData;
      const userId = resourceData.id;
      
      // Get updated user status
      const userStatus = await getUserState(`communications/presences/${userId}`);
      
      if (userStatus) {
        // Create status notification card
        const statusCard = createStatusCard(
          "Your Status Changed!", 
          userStatus.availability, 
          userStatus.activity
        );
        
        // Send notification to all stored conversation references
        const notificationMessage = "Change your status to get notification";
        
        for (const conversationReference of Object.values(conversationReferences)) {
          await sendProactiveMessage(conversationReference, notificationMessage);
          await sendProactiveMessage(conversationReference, statusCard);
        }
      }
    }
  } catch (error) {
    console.error('Error handling notification:', error);
  }
};

// Setup custom routes
setupCustomRoutes();

function setupCustomRoutes() {
  const expressApp = app.http.express;

  if (expressApp) {
    const express = require('express');
    const path = require('path');

    // Use body parsing middleware
    expressApp.use(express.json());
    expressApp.use(express.urlencoded({ extended: true }));

    // Serve static files (e.g., auth-end.html)
    expressApp.use(express.static(path.join(__dirname, 'public')));

    // Webhook endpoint for Graph change notifications
    expressApp.post('/api/notifications', async (req, res) => {
      console.log('Received notification:', req.body);
      
      // Validate notification (Graph subscription validation)
      if (req.query && req.query.validationToken) {
        console.log('Validation token received:', req.query.validationToken);
        res.status(200).send(req.query.validationToken);
        return;
      }
      
      // Process the notification
      try {
        await handleNotification(req.body);
        res.status(202).send('Notification processed');
      } catch (error) {
        console.error('Error processing notification:', error);
        res.status(500).send('Error processing notification');
      }
    });
  }
}

module.exports = app;
