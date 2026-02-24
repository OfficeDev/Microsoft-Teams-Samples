/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * 
 * Bot Auth Quickstart - Teams SDK V2
 * This sample demonstrates implementing SSO in Microsoft Teams using Azure AD.
 * It also includes proactive app installation and messaging capabilities.
 */

import { stripMentionsText } from "@microsoft/teams.api";
import { App } from "@microsoft/teams.apps";
import { LocalStorage } from "@microsoft/teams.common";
import * as endpoints from '@microsoft/teams.graph-endpoints';
import config from "./config";
import { ManagedIdentityCredential } from "@azure/identity";

const storage = new LocalStorage();

interface UserState {
  waitingForTokenConfirmation: boolean;
}

// Get or create user state for a given user ID
const getUserState = (userId: string): UserState => {
  const key = `user_${userId}`;
  let state = storage.get(key) as UserState | undefined;
  if (!state) {
    state = { waitingForTokenConfirmation: false };
    storage.set(key, state);
  }
  return state;
};

// Save user state for a given user ID
const setUserState = (userId: string, state: UserState): void => {
  const key = `user_${userId}`;
  storage.set(key, state);
};

// Create token factory for managed identity
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
  appConfig.clientId = config.MicrosoftAppId;
  appConfig.token = createTokenFactory();
  appConfig.appType = "UserAssignedMSI";
} else {
  appConfig.clientId = config.MicrosoftAppId;
  appConfig.clientSecret = config.MicrosoftAppPassword;
  appConfig.tenantId = config.MicrosoftAppTenantId;
  appConfig.appType = config.MicrosoftAppType || "SingleTenant";
}

const app = new App(appConfig);

// Handle logout command
async function handleLogout(context: any): Promise<boolean> {
  await context.signout();
  await context.send('You have been signed out.');
  return true;
}

// Handle login command
async function handleLogin(context: any, isExplicitLoginCommand: boolean): Promise<boolean> {
  try {
    await context.signin();
  } catch (error: any) {
    // Sign-in error handled by Teams SDK
  }
  return true;
}

// Handle token confirmation response (Yes/No)
async function handleTokenConfirmation(context: any, textLower: string, userId: string, userState: UserState): Promise<boolean> {
  if (!userState.waitingForTokenConfirmation) {
    return false;
  }

  if (textLower === 'yes') {
    await context.send('Authentication is working correctly and your access token has been securely stored.');
  } else if (textLower === 'no') {
    await context.send('Thank you.');
  } else {
    return false;
  }

  userState.waitingForTokenConfirmation = false;
  setUserState(userId, userState);
  return true;
}

// Display user details after successful sign-in
async function displayUserDetails(context: any, userId: string) {
  const { send, userGraph } = context;
  const userState = getUserState(userId);

  if (!userGraph) {
    return;
  }

  try {
    const me = await userGraph.call(endpoints.me.get);
    const jobTitle = me.jobTitle || null;
    
    await send(`You're logged in as ${me.displayName} (${me.userPrincipalName}); your job title is: ${jobTitle}`);
    
    await send('Would you like to view your token?\n\n**Yes or No**');
    
    userState.waitingForTokenConfirmation = true;
    setUserState(userId, userState);
  } catch (error: any) {
    // Error fetching user details - handled silently
  }
}

// Handle install add event - welcome new users
app.on('conversationUpdate', async (context) => {
  const activity = context.activity;
  
  const membersAdded = activity.membersAdded;
  if (membersAdded) {
    for (const member of membersAdded) {
      if (member.id !== activity.recipient?.id) {
        await context.send('Welcome to TeamsBot! Type anything to get logged in. Type \'logout\' to sign-out.\n\nCommands:\n- **Login**: Sign in to the bot\n- **Logout**: Sign out from the bot');
      }
    }
  }
});

// Handle successful sign in event
app.event('signin', async (context) => {
  const userId = context.activity.from.id;
  
  await context.send('You have been signed in successfully!');
  await displayUserDetails(context, userId);
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

  await displayUserDetails(context, userId);
});

export default app;
