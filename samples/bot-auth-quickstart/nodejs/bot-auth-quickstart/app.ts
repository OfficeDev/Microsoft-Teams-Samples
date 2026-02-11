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
const conversationReferences = new Map<string, any>();

// Cache for the dynamically fetched catalog ID
let cachedCatalogId: string | null = null;

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

/**
 * Get app-only access token using client credentials flow.
 * This token has Application permissions and can perform admin operations.
 */
async function getAppOnlyToken(): Promise<string> {
  const tokenUrl = `https://login.microsoftonline.com/${config.MicrosoftAppTenantId}/oauth2/v2.0/token`;
  
  const response = await fetch(tokenUrl, {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
    },
    body: new URLSearchParams({
      client_id: config.MicrosoftAppId!,
      client_secret: config.MicrosoftAppPassword!,
      scope: "https://graph.microsoft.com/.default",
      grant_type: "client_credentials",
    }),
  });

  if (!response.ok) {
    throw new Error(`Failed to get app-only token: ${response.statusText}`);
  }

  const data = await response.json();
  return data.access_token;
}

/**
 * Discovers the catalog app ID for this bot using multiple strategies:
 * 1. Return cached value if available
 * 2. Fall back to APP_CATALOG_TEAM_APP_ID environment variable
 * 3. Look up from a known user's installed apps using app-only token
 *    (uses Application permission TeamsAppInstallation.ReadWriteForUser.All)
 * 
 * Uses app-only token (client credentials) like Python implementation.
 */
async function getCatalogTeamAppId(knownUserId?: string): Promise<string> {
  // Return cached value if available
  if (cachedCatalogId) {
    return cachedCatalogId;
  }

  // Fall back to environment variable if configured
  if (config.appCatalogTeamAppId) {
    cachedCatalogId = config.appCatalogTeamAppId;
    return cachedCatalogId;
  }

  // Strategy: Look up from a known user's installed apps using app-only token
  if (!config.teamsAppId && !config.MicrosoftAppId) {
    throw new Error(
      "Cannot discover catalog ID: Neither TEAMS_APP_ID nor CLIENT_ID is configured."
    );
  }

  if (!knownUserId) {
    throw new Error(
      "Cannot discover catalog ID: No known user ID provided. " +
      "Please set APP_CATALOG_TEAM_APP_ID in your .localConfigs file."
    );
  }

  try {
    // Get app-only token
    const appToken = await getAppOnlyToken();
    
    // Fetch all installed apps with teamsApp expansion for the known user
    const url =
      `https://graph.microsoft.com/v1.0/users/${knownUserId}/teamwork/installedApps?$expand=teamsApp`;
    
    const response = await fetch(url, {
      headers: {
        Authorization: `Bearer ${appToken}`,
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Failed to query installed apps: ${response.status} ${response.statusText} - ${errorText}`);
    }

    const data = await response.json();
    const installedApps = data.value || [];

    // Search for our bot by matching externalId
    for (const appEntry of installedApps) {
      const teamsApp = appEntry.teamsApp || {};
      const externalId = teamsApp.externalId || '';
      const displayName = teamsApp.displayName || '';
      const catalogId = teamsApp.id || '';

      // Match by CLIENT_ID (BOT_ID) or TEAMS_APP_ID
      if (externalId && (externalId === config.MicrosoftAppId || externalId === config.teamsAppId)) {
        cachedCatalogId = catalogId;
        return cachedCatalogId;
      }
    }

    throw new Error(
      `Bot not found in user's installed apps. ` +
      `Make sure the app is installed and the TEAMS_APP_ID (${config.teamsAppId}) or CLIENT_ID (${config.MicrosoftAppId}) is correct.`
    );
  } catch (error: any) {
    throw new Error(
      `Failed to discover catalog ID: ${error.message}. ` +
      `You can manually set APP_CATALOG_TEAM_APP_ID in your .localConfigs file.`
    );
  }
}

// Install the Teams app for a specific user using Graph API
async function installAppForUser(userId: string, userToken: string, catalogId: string): Promise<number> {
  try {
    // Get app-only token for installation (requires Application permission)
    const appToken = await getAppOnlyToken();
    
    // Check if app is already installed
    const checkResponse = await fetch(
      `https://graph.microsoft.com/v1.0/users/${userId}/teamwork/installedApps?$expand=teamsAppDefinition`,
      {
        headers: {
          Authorization: `Bearer ${appToken}`,
        },
      }
    );

    if (checkResponse.ok) {
      const data = await checkResponse.json();
      const installedApps = data.value || [];
      
      // Check if our app is already installed
      for (const app of installedApps) {
        const teamsApp = app.teamsAppDefinition?.teamsApp || {};
        if (teamsApp.externalId === config.MicrosoftAppId || teamsApp.externalId === config.teamsAppId) {
          return 409;
        }
      }
    }

    // Install the app using app-only token
    const installResponse = await fetch(
      `https://graph.microsoft.com/v1.0/users/${userId}/teamwork/installedApps`,
      {
        method: "POST",
        headers: {
          Authorization: `Bearer ${appToken}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          "teamsApp@odata.bind": `https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/${catalogId}`,
        }),
      }
    );

    if (installResponse.ok || installResponse.status === 201) {
      return 201;
    } else if (installResponse.status === 409) {
      // 409 Conflict means app is already installed
      return 409;
    } else {
      const errorText = await installResponse.text();
      throw new Error(`Failed to install app: ${installResponse.status} ${installResponse.statusText} - ${errorText}`);
    }
  } catch (error: any) {
    throw error;
  }
}

// Check and install the app for all members in a team/chat
async function checkAndInstallForAllMembers(context: any, userToken: string): Promise<{ newInstalls: number; existing: number; errors: string[] }> {
  let newInstalls = 0;
  let existing = 0;
  const errors: string[] = [];

  try {
    const activity = context.activity;
    const conversationId = activity.conversation.id;
    const conversationType = activity.conversation.conversationType;

    // Get app-only token for reading members
    const appToken = await getAppOnlyToken();
    let members: any[] = [];

    // Get members from Graph API based on conversation type
    try {
      let membersUrl = '';
      
      if (conversationType === "channel") {
        // For Teams channels, extract team ID
        const teamId = conversationId.split(';')[0];
        membersUrl = `https://graph.microsoft.com/v1.0/teams/${teamId}/members`;
      } else if (conversationType === "groupChat") {
        // For group chats, use the full conversation ID
        membersUrl = `https://graph.microsoft.com/v1.0/chats/${conversationId}/members`;
      } else {
        // For 1:1 chats, we don't need to install for multiple members
        errors.push('Installation not supported for 1:1 conversations');
        return { newInstalls, existing, errors };
      }

      const response = await fetch(membersUrl, {
        headers: {
          Authorization: `Bearer ${appToken}`,
        },
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Failed to get members: ${response.status} ${response.statusText} - ${errorText}`);
      }

      const data = await response.json();
      members = data.value || [];

      // Get a known user ID (first member with userId) for catalog discovery
      let knownUserId: string | undefined;
      for (const member of members) {
        if (member.userId) {
          knownUserId = member.userId;
          break;
        }
      }
      
      // Get catalog ID using a known user (who has the app installed)
      const catalogId = await getCatalogTeamAppId(knownUserId);

      for (const member of members) {
        const userId = member.userId;
        const displayName = member.displayName || "Unknown";

        if (!userId) {
          continue;
        }

        try {
          const statusCode = await installAppForUser(userId, userToken, catalogId);
          if (statusCode === 201) {
            newInstalls++;
          } else if (statusCode === 409) {
            existing++;
          }
        } catch (error: any) {
          errors.push(`${displayName}: ${error.message}`);
        }
      }
    } catch (getMembersError: any) {
      errors.push(`Failed to get members: ${getMembersError.message}`);
    }

    return { newInstalls, existing, errors };
  } catch (error: any) {
    throw error;
  }
}

// Send a proactive message to all members in a team/chat
async function sendProactiveMessageToAll(context: any, userToken: string): Promise<{ sent: number; errors: string[] }> {
  let sent = 0;
  const errors: string[] = [];

  try {
    const activity = context.activity;
    const conversationId = activity.conversation.id;
    const conversationType = activity.conversation.conversationType;

    // Get app-only token for reading members
    const appToken = await getAppOnlyToken();
    let members: any[] = [];
    let membersUrl = '';

    if (conversationType === "channel") {
      const teamId = conversationId.split(';')[0];
      membersUrl = `https://graph.microsoft.com/v1.0/teams/${teamId}/members`;
    } else if (conversationType === "groupChat") {
      membersUrl = `https://graph.microsoft.com/v1.0/chats/${conversationId}/members`;
    } else {
      errors.push('Proactive messaging not supported for 1:1 conversations');
      return { sent, errors };
    }

    const response = await fetch(membersUrl, {
      headers: { Authorization: `Bearer ${appToken}` }
    });
    
    if (response.ok) {
      const data = await response.json();
      members = data.value || [];
    } else {
      const errorText = await response.text();
      errors.push(`Failed to get members: ${response.status} ${response.statusText}`);
      return { sent, errors };
    }

    // Build a message mentioning all members
    let messageText = "Hello everyone! Here's a message for all members:\n\n";
    
    for (const member of members) {
      const userId = member.userId;
      const displayName = member.displayName || "Unknown";

      if (!userId) continue;

      try {
        // Add mention for each user
        messageText += `<at>${displayName}</at> `;
        sent++;
      } catch (error: any) {
        errors.push(`${displayName}: ${error.message}`);
      }
    }

    // Send the message in the current conversation
    try {
      await context.send(messageText);
    } catch (error: any) {
      errors.push(`Failed to send message: ${error.message}`);
      sent = 0;
    }

    return { sent, errors };
  } catch (error: any) {
    throw error;
  }
}

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
    const userToken = context.userToken;
    if (userToken) {
      await context.send(`Here is your token: ${userToken}`);
    } else {
      await context.send('Token is available but not accessible in this context. Authentication is working correctly.');
    }
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
async function displayUserDetails(context: any, token: string, userId: string) {
  const { send, userGraph } = context;
  const userState = getUserState(userId);

  if (!userGraph) {
    return;
  }

  try {
    const me = await userGraph.call(endpoints.me.get);
    const jobTitle = me.jobTitle || null;
    
    await send(`You're logged in as ${me.displayName} (${me.userPrincipalName}); your job title is: ${jobTitle}; your photo is:`);
    
    try {
      // Fetch photo bytes directly from Graph API
      const photoUrl = 'https://graph.microsoft.com/v1.0/me/photo/$value';
      const response = await fetch(photoUrl, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'image/jpeg'
        }
      });
      
      if (response.ok) {
        const arrayBuffer = await response.arrayBuffer();
        const photoBuffer = Buffer.from(arrayBuffer);
        const base64Image = photoBuffer.toString('base64');
        
        await send(`<img src="data:image/jpeg;base64,${base64Image}" alt="Profile Photo" />`);
      }
    } catch (photoError: any) {
      // Photo not available - silently continue
    }
    
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
  storeConversationReference(activity);
  
  const membersAdded = activity.membersAdded;
  if (membersAdded) {
    for (const member of membersAdded) {
      if (member.id !== activity.recipient?.id) {
        await context.send('Welcome to TeamsBot with Proactive Installation! Type anything to get logged in. Type \'logout\' to sign-out.\n\nCommands:\n- **Login**: Sign in to the bot\n- **Check and Install** or **Install**: Install the app for all members in the team/chat\n- **Send message** or **Send**: Send a proactive message to all members\n- **Logout**: Sign out from the bot');
      }
    }
  }
});

// Handle successful sign in event
app.event('signin', async (context) => {
  const token = context.userToken;
  const userId = context.activity.from.id;
  
  await context.send('You have been signed in successfully!');
  await displayUserDetails(context, token, userId);
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
      // Error handled through result.errors array
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
      // Error handled through result.errors array
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

  const userToken = context.userToken;
  await displayUserDetails(context, userToken, userId);
});

export default app;
