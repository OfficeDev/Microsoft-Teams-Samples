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
  storage
};

if (config.MicrosoftAppType === "UserAssignedMsi") {
  // Use managed identity for Azure deployments
  appConfig.clientId = config.MicrosoftAppId;
  appConfig.token = createTokenFactory();
  appConfig.appType = "UserAssignedMSI";
  // Only use OAuth connection in Azure where it's provisioned
  appConfig.oauth = {
    defaultConnectionName: config.connectionName || 'graph'
  };
} else {
  // Use client secret for local development
  // DON'T use OAuth connection locally - Teams SSO handles auth automatically
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

// Handle logout command
app.message('logout', async (context) => {
  const { isSignedIn, signout, send } = context;
  
  if (!isSignedIn) {
    await send('You are not signed in.');
    return;
  }
  
  await signout();
  await send('You have been signed out.');
});

// Handle all messages
app.on('message', async (context) => {
  const activity = context.activity;
  const text: string = stripMentionsText(activity);
  const textLower = text.toLowerCase();
  
  const { isSignedIn, signin, send, userGraph } = context;
  
  const userId = activity.from.id;
  const userState = getUserState(userId);
  
  // Get the user token from context
  const userToken = context.userToken;

  // Handle logout command
  if (textLower === 'logout') {
    if (!isSignedIn) {
      await send('You are not signed in.');
      return;
    }
    
    await context.signout();
    await send('You have been signed out.');
    return;
  }

  // Check if user is responding to token confirmation
  if (userState.waitingForTokenConfirmation && isSignedIn) {
    // Handle both text input and button submit actions
    const responseText = (activity.value as any)?.text || textLower;
    
    if (responseText === 'yes' || responseText === 'y') {
      if (userToken) {
        await send(`Here is your token: ${userToken}`);
      } else {
        await send('Token is available but not accessible in this context. Authentication is working correctly.');
      }
    } else {
      await send('Thank you.');
    }
    
    // Reset state
    userState.waitingForTokenConfirmation = false;
    setUserState(userId, userState);
    return;
  }

  // Check if user is signed in, if not prompt for signin
  if (!isSignedIn) {
    await signin();
    return;
  }

  // User is signed in, process the message
  try {
    // Use Teams SDK userGraph helper for Graph API calls
    if (userGraph) {
      try {
        // Use the userGraph helper - pass the endpoint function itself, not the result
        const me = await userGraph.call(endpoints.me.get);
        const jobTitle = me.jobTitle || null;
        
        // Format the response to match the screenshot
        await send(`You're logged in as ${me.displayName} (${me.userPrincipalName}); your job title is: ${jobTitle}; your photo is:`);
        
        // Note: Photo display removed to avoid payload size issues with Bot Framework
        // The Teams SDK successfully authenticates and retrieves user data via OAuth
        
        // Ask if they want to see their token
        await send('Would you like to view your token?');
        userState.waitingForTokenConfirmation = true;
        setUserState(userId, userState);
        
        return;
      } catch (graphError: any) {
        console.error('Error using userGraph:', graphError);
        await send(`Error fetching profile: ${graphError.message}`);
        return;
      }
    } else {
      await send('OAuth authentication is not properly configured.');
      return;
    }
  } catch (error: any) {
    console.error('Error processing message:', error);
    await send(`Login was not successful, please try again.`);
  }
});

export default app;
