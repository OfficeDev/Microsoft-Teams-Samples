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
  const membersAdded = context.activity.membersAdded;
  if (membersAdded) {
    for (const member of membersAdded) {
      if (member.id !== context.activity.recipient?.id) {
        await context.send('Welcome to TeamsBot. Type anything to get logged in. Type \'logout\' to sign-out.');
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

  // Handle logout/signout commands
  if (textLower === 'logout' || textLower === 'signout') {
    await handleLogout(context);
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
