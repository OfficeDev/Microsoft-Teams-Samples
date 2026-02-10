// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { stripMentionsText } from "@microsoft/teams.api";
import { App } from "@microsoft/teams.apps";
import { LocalStorage } from "@microsoft/teams.common";
import * as endpoints from '@microsoft/teams.graph-endpoints';
import config from "./config";
import { ManagedIdentityCredential } from "@azure/identity";

// Create storage for user and conversation state
const storage = new LocalStorage();

// Store conversation references for proactive messaging
const conversationReferences = new Map<string, any>();

const createTokenFactory = () => {
  return async (scope: string | string[], tenantId?: string): Promise<string> => {
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

// Configure authentication based on app type
interface AppConfig {
  storage: LocalStorage;
  oauth?: {
    defaultConnectionName: string;
  };
  clientId?: string;
  token?: (scope: string | string[], tenantId?: string) => Promise<string>;
  appType?: string;
  clientSecret?: string;
  tenantId?: string;
}

let appConfig: AppConfig = {
  storage,
  oauth: {
    defaultConnectionName: config.connectionName || 'graph'
  }
};

if (config.MicrosoftAppType === "UserAssignedMsi") {
  // Use managed identity for Azure deployments
  appConfig.clientId = config.MicrosoftAppId;
  appConfig.token = createTokenFactory();
  appConfig.appType = "UserAssignedMSI";
} else {
  // Use client secret for local development
  appConfig.clientId = config.MicrosoftAppId;
  appConfig.clientSecret = config.MicrosoftAppPassword;
  appConfig.tenantId = config.MicrosoftAppTenantId;
  appConfig.appType = config.MicrosoftAppType || "SingleTenant";
}

// Create the app with storage and OAuth configuration
const app = new App(appConfig);

// Interface for user state
interface UserState {
  waitingForTokenConfirmation: boolean;
}

// Track user state for token confirmation flow
const getUserState = (userId: string): UserState => {
  const key = `user_${userId}`;
  let state = storage.get(key) as UserState | undefined;
  if (!state) {
    state = { waitingForTokenConfirmation: false };
    storage.set(key, state);
  }
  return state;
};

const setUserState = (userId: string, state: UserState): void => {
  const key = `user_${userId}`;
  storage.set(key, state);
};

// Store conversation reference for proactive messaging
const storeConversationReference = (activity: any): void => {
  const userId = activity.from.aadObjectId || activity.from.id;
  conversationReferences.set(userId, {
    userId: activity.from.id,
    conversationId: activity.conversation.id,
    serviceUrl: activity.serviceUrl,
    tenantId: activity.conversation.tenantId || "",
  });
};

// Install app for a specific user using Graph API
async function installAppForUser(userId: string, userToken: string): Promise<number> {
  if (!config.appCatalogTeamAppId) {
    throw new Error("APP_CATALOG_TEAM_APP_ID is not configured");
  }

  try {
    // Check if app is already installed
    const checkResponse = await fetch(
      `https://graph.microsoft.com/v1.0/users/${userId}/teamwork/installedApps?$expand=teamsAppDefinition&$filter=teamsAppDefinition/teamsAppId eq '${config.appCatalogTeamAppId}'`,
      {
        headers: {
          Authorization: `Bearer ${userToken}`,
        },
      }
    );

    if (checkResponse.ok) {
      const data = await checkResponse.json();
      if (data.value && data.value.length > 0) {
        console.log(`App already installed for user ${userId}`);
        return 409; // Already installed
      }
    }

    // Install app for user
    const installResponse = await fetch(
      `https://graph.microsoft.com/v1.0/users/${userId}/teamwork/installedApps`,
      {
        method: "POST",
        headers: {
          Authorization: `Bearer ${userToken}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          "teamsApp@odata.bind": `https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/${config.appCatalogTeamAppId}`,
        }),
      }
    );

    if (installResponse.ok) {
      console.log(`Successfully installed app for user ${userId}`);
      return 201; // Newly installed
    } else {
      const errorText = await installResponse.text();
      throw new Error(`Failed to install app: ${installResponse.statusText} - ${errorText}`);
    }
  } catch (error: any) {
    console.error(`Error installing app for user ${userId}:`, error);
    throw error;
  }
}

// Check and install app for all members in team/chat
async function checkAndInstallForAllMembers(context: any, userToken: string): Promise<{ newInstalls: number; existing: number; errors: string[] }> {
  let newInstalls = 0;
  let existing = 0;
  const errors: string[] = [];

  try {
    // Get team/chat members using Teams context
    const activity = context.activity;
    const conversationId = activity.conversation.id;
    const tenantId = activity.conversation.tenantId;

    // Fetch members from Graph API based on conversation type
    let members: any[] = [];
    const conversationType = activity.conversation.conversationType;

    if (conversationType === "channel") {
      // Team scenario
      const teamId = conversationId;
      const response = await fetch(
        `https://graph.microsoft.com/v1.0/teams/${teamId}/members`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.ok) {
        const data = await response.json();
        members = data.value || [];
      }
    } else {
      // Group chat scenario
      const chatId = conversationId;
      const response = await fetch(
        `https://graph.microsoft.com/v1.0/chats/${chatId}/members`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.ok) {
        const data = await response.json();
        members = data.value || [];
      }
    }

    // Install app for each member
    const installPromises = members.map(async (member) => {
      try {
        const userId = member.userId;
        const statusCode = await installAppForUser(userId, userToken);
        return { status: statusCode, name: member.displayName };
      } catch (error: any) {
        return { status: 'error', name: member.displayName, error: error.message };
      }
    });

    const results = await Promise.all(installPromises);

    results.forEach((result) => {
      if (result.status === 201) {
        newInstalls++;
      } else if (result.status === 409) {
        existing++;
      } else {
        errors.push(`${result.name}: ${result.error}`);
      }
    });

    return { newInstalls, existing, errors };
  } catch (error: any) {
    console.error("Error in checkAndInstallForAllMembers:", error);
    throw error;
  }
}

// Send proactive message to all members
async function sendProactiveMessageToAll(context: any, userToken: string): Promise<{ sent: number; errors: string[] }> {
  let sent = 0;
  const errors: string[] = [];

  try {
    const activity = context.activity;
    const conversationId = activity.conversation.id;
    const conversationType = activity.conversation.conversationType;

    // Fetch members from Graph API
    let members: any[] = [];

    if (conversationType === "channel") {
      // Team scenario
      const teamId = conversationId;
      const response = await fetch(
        `https://graph.microsoft.com/v1.0/teams/${teamId}/members`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.ok) {
        const data = await response.json();
        members = data.value || [];
      }
    } else {
      // Group chat scenario
      const chatId = conversationId;
      const response = await fetch(
        `https://graph.microsoft.com/v1.0/chats/${chatId}/members`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.ok) {
        const data = await response.json();
        members = data.value || [];
      }
    }

    // Send message to each member using adapter's createConversation
    for (const member of members) {
      try {
        const ref = {
          ...context.activity.getConversationReference(),
          user: {
            id: member.userId,
            name: member.displayName,
          },
        };

        await context.adapter.createConversation(ref, async (turnContext: any) => {
          await turnContext.sendActivity("Proactive hello.");
        });

        sent++;
      } catch (error: any) {
        console.error(`Failed to send message to ${member.displayName}:`, error);
        errors.push(`${member.displayName}: ${error.message}`);
      }
    }

    return { sent, errors };
  } catch (error: any) {
    console.error("Error in sendProactiveMessageToAll:", error);
    throw error;
  }
}

// Helper function to handle logout
async function handleLogout(context: any): Promise<boolean> {
  const { isSignedIn, send } = context;
  
  if (!isSignedIn) {
    await send('You are not signed in.');
    return true;
  }
  
  await context.signout();
  await send('You have been signed out.');
  return true;
}

// Helper function to handle login
async function handleLogin(context: any, isExplicitLoginCommand: boolean): Promise<boolean> {
  const { isSignedIn, signin, send } = context;
  
  if (isSignedIn && isExplicitLoginCommand) {
    await send('You are already signed in.');
    return true;
  }
  
  if (!isSignedIn) {
    try {
      await signin();
    } catch (error: any) {
      console.error('Sign-in error:', error);
    }
    return true;
  }
  
  return false;
}

// Helper function to handle token confirmation response
async function handleTokenConfirmation(context: any, textLower: string, userId: string, userState: UserState): Promise<boolean> {
  const { send } = context;
  const userToken = context.userToken;
  
  if (!userState.waitingForTokenConfirmation || !context.isSignedIn) {
    return false;
  }
  
  if (textLower === 'yes') {
    if (userToken) {
      await send(`Here is your token: ${userToken}`);
    } else {
      await send('Token is available but not accessible in this context. Authentication is working correctly.');
    }
  } else if (textLower === 'no') {
    await send('Thank you.');
  }
  
  // Reset state
  userState.waitingForTokenConfirmation = false;
  setUserState(userId, userState);
  return true;
}

// Handle conversation updates (members added)
app.on('conversationUpdate', async (context) => {
  const activity = context.activity;
  
  // Store conversation reference whenever there's a conversation update
  storeConversationReference(activity);
  
  const membersAdded = activity.membersAdded;
  if (membersAdded) {
    for (const member of membersAdded) {
      if (member.id !== activity.recipient?.id) {
        await context.send('Welcome to TeamsBot with Proactive Installation! Type anything to get logged in. Type \'logout\' to sign-out.\n\nCommands:\n- **Check and Install** or **Install**: Install the app for all members in the team/chat\n- **Send message** or **Send**: Send a proactive message to all members\n- **Login**: Sign in to the bot\n- **Logout**: Sign out from the bot');
      }
    }
  }
});

// Function to display user details
async function displayUserDetails(context: any) {
  const { send, userGraph } = context;
  const userToken = context.userToken;
  const userId = context.activity.from.id;
  const userState = getUserState(userId);

  if (!userGraph) {
    console.error('OAuth authentication is not properly configured.');
    return;
  }

  try {
    // Get user info
    const me = await userGraph.call(endpoints.me.get);
    const jobTitle = me.jobTitle || null;
    
    // Send user details
    await send(`You're logged in as ${me.displayName} (${me.userPrincipalName}); your job title is: ${jobTitle}; your photo is:`);
    
    // Fetch and display user's profile photo
    if (userToken) {
      try {
        const photoResponse = await fetch('https://graph.microsoft.com/v1.0/me/photo/$value', {
          headers: {
            'Authorization': `Bearer ${userToken}`
          }
        });
        
        if (photoResponse.ok) {
          const photoBlob = await photoResponse.blob();
          const buffer = Buffer.from(await photoBlob.arrayBuffer());
          const base64Image = buffer.toString('base64');
          await send(`![Profile Photo](data:image/jpeg;base64,${base64Image})`);
        } else {
          console.log('Profile photo not available. Status:', photoResponse.status);
        }
      } catch (photoError: any) {
        console.error('Error fetching profile photo:', photoError);
      }
    } else {
      console.log('User token not available, skipping photo fetch');
    }
    
    // Ask for token confirmation
    await send('Would you like to view your token?\n\n**Yes or No**');
    
    userState.waitingForTokenConfirmation = true;
    setUserState(userId, userState);
  } catch (error: any) {
    console.error('Error fetching user details:', error);
  }
}

// Handle successful sign-in
app.event('signin', async (context) => {
  console.log('User signed in successfully');
  await context.send('You have been signed in successfully!');
  
  // Automatically display user details after successful sign-in
  await displayUserDetails(context);
});

// Handle all messages
app.on('message', async (context) => {
  const activity = context.activity;
  const text: string = stripMentionsText(activity) || '';
  const textLower = text.toLowerCase().trim();
  
  const { isSignedIn, send } = context;
  
  const userId = activity.from.id;
  const userState = getUserState(userId);

  // Store conversation reference for proactive messaging
  storeConversationReference(activity);

  // Handle logout/signout commands
  if (textLower === 'logout' || textLower === 'signout') {
    await handleLogout(context);
    return;
  }

  // Handle check and install command (requires authentication)
  if (textLower === 'check and install' || textLower === 'install') {
    if (!isSignedIn) {
      await send('Please sign in first to use this feature.');
      await handleLogin(context, false);
      return;
    }

    try {
      const userToken = context.userToken;
      if (!userToken) {
        await send('Unable to get authentication token. Please try signing in again.');
        return;
      }

      await send('Checking and installing app for all members...');
      const result = await checkAndInstallForAllMembers(context, userToken);
      
      let message = `**Installation Complete**\n\nExisting: ${result.existing}\nNewly Installed: ${result.newInstalls}`;
      
      if (result.errors.length > 0) {
        message += `\n\n**Errors:**\n${result.errors.join('\n')}`;
      }
      
      await send(message);
    } catch (error: any) {
      console.error('Error in check and install:', error);
      await send(`Error installing app: ${error.message}`);
    }
    return;
  }

  // Handle send message command (requires authentication)
  if (textLower === 'send message' || textLower === 'send') {
    if (!isSignedIn) {
      await send('Please sign in first to use this feature.');
      await handleLogin(context, false);
      return;
    }

    try {
      const userToken = context.userToken;
      if (!userToken) {
        await send('Unable to get authentication token. Please try signing in again.');
        return;
      }

      await send('Sending proactive messages to all members...');
      const result = await sendProactiveMessageToAll(context, userToken);
      
      let message = `**Messages Sent:** ${result.sent}`;
      
      if (result.errors.length > 0) {
        message += `\n\n**Errors:**\n${result.errors.join('\n')}`;
      }
      
      await send(message);
    } catch (error: any) {
      console.error('Error in send message:', error);
      await send(`Error sending messages: ${error.message}`);
    }
    return;
  }

  // Check if user is responding to token confirmation
  if (await handleTokenConfirmation(context, textLower, userId, userState)) {
    return;
  }

  // Handle login/signin or check if user is signed in
  const isExplicitLoginCommand = textLower === 'login' || textLower === 'signin';
  if (!isSignedIn || isExplicitLoginCommand) {
    await handleLogin(context, isExplicitLoginCommand);
    return;
  }

  // User is signed in, display user details
  await displayUserDetails(context);
});

export default app;
