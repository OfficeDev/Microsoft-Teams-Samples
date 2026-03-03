/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 *
 * Bot Auth Quickstart - Teams SDK V2
 * This sample demonstrates implementing SSO in Microsoft Teams using Azure AD.
 */

import { stripMentionsText } from "@microsoft/teams.api";
import { App } from "@microsoft/teams.apps";
import { LocalStorage } from "@microsoft/teams.common";
import * as endpoints from '@microsoft/teams.graph-endpoints';
import config from "./config";
import { ManagedIdentityCredential } from "@azure/identity";

const storage = new LocalStorage();

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
 * Helper function to handle authentication and create Graph client using Token pattern.
 * Returns the userGraph client if authenticated, or null if sign-in is required.
 */
async function getAuthenticatedGraphClient(context: any): Promise<any | null> {
  if (!context.isSignedIn) {
    await context.send("🔐 Please sign in first to access Microsoft Graph.");
    await context.signin();
    return null;
  }

  try {
    return context.userGraph;
  } catch (error: any) {
    context.logger?.error(`Failed to create Graph client: ${error}`);
    await context.send("🔐 Failed to create authenticated client. Trying to sign in again.");
    await context.signin();
    return null;
  }
}

// Handle successful sign-in event
app.event('signin', async (context) => {
  await context.send(
    "✅ **Successfully signed in!**\n\n" +
    "You can now use these commands:\n\n" +
    "• **profile** - View your profile\n\n" +
    "• **signout** - Sign out when done"
  );
});

// Handle all messages
app.on('message', async (context) => {
  const activity = context.activity;
  const text: string = stripMentionsText(activity) || '';
  const textLower = text.toLowerCase().trim();

  const { isSignedIn, send } = context;

  // Handle signin command
  if (textLower === 'signin') {
    if (isSignedIn) {
      await send("✅ You are already signed in!");
    } else {
      await send("🔐 Signing you in to access Microsoft Graph...");
      await context.signin();
    }
    return;
  }

  // Handle signout command
  if (textLower === 'signout') {
    if (!isSignedIn) {
      await send("ℹ️ You are not currently signed in.");
    } else {
      await context.signout();
      await send("👋 You have been signed out successfully!");
    }
    return;
  }

  // Handle profile command
  if (textLower === 'profile') {
    const graph = await getAuthenticatedGraphClient(context);
    if (!graph) return;

    try {
      const me = await graph.call(endpoints.me.get);
      if (me) {
        const profileInfo =
          `👤 **Your Profile**\n\n` +
          `**Name:** ${me.displayName || 'N/A'}\n\n` +
          `**Email:** ${me.userPrincipalName || 'N/A'}\n\n` +
          `**Job Title:** ${me.jobTitle || 'N/A'}\n\n` +
          `**Department:** ${me.department || 'N/A'}\n\n` +
          `**Office:** ${me.officeLocation || 'N/A'}`;
        await send(profileInfo);
      } else {
        await send("❌ Could not retrieve your profile information.");
      }
    } catch (error: any) {
      if (error?.code === 'AuthenticationError' || error?.statusCode === 401) {
        context.logger?.error(`Authentication error: ${error}`);
        await send("🔐 Authentication failed. Please try signing in again.");
      } else {
        context.logger?.error(`Error getting profile: ${error}`);
        await send(`❌ Failed to get your profile: ${error?.message || error}`);
      }
    }
    return;
  }

  // Default message - show available commands
  await send(
    "👋 **Hello! I'm a Teams Auth Quickstart and Graph bot.**\n\n" +
    "**Available commands:**\n\n" +
    "• **signin** - Sign in to your Microsoft account\n\n" +
    "• **signout** - Sign out\n\n" +
    "• **profile** - Show your profile information\n\n"
  );
});

// Handle error events
app.event('error', async (event: any) => {
  console.error(`Error occurred: ${event.error}`);
  if (event.context) {
    console.error(`Context: ${event.context}`);
  }
});

export default app;
