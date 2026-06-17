/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 *
 * Bot Auth Quickstart - Teams SDK V2
 * This sample demonstrates implementing SSO in Microsoft Teams using Azure AD.
 */

import { stripMentionsText } from "@microsoft/teams.api";
import { App } from "@microsoft/teams.apps";
import * as endpoints from '@microsoft/teams.graph-endpoints';

const app = new App({
  oauth: {
    defaultConnectionName: process.env.CONNECTION_NAME || 'graph'
  }
});

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

// Dispatch incoming messages to the appropriate handler
app.on('message', async (context) => {
  const text: string = stripMentionsText(context.activity) || '';
  const command = text.toLowerCase().trim();

  if (command === 'signin') return handleSignin(context);
  if (command === 'signout') return handleSignout(context);
  if (command === 'profile') return handleProfile(context);
  return handleDefault(context);
});

// Handler for signin command
async function handleSignin(context: any): Promise<void> {
  if (context.isSignedIn) {
    await context.send("✅ You are already signed in!");
  } else {
    await context.send("🔐 Signing you in to access Microsoft Graph...");
    await context.signin();
  }
}

// Handler for signout command
async function handleSignout(context: any): Promise<void> {
  if (!context.isSignedIn) {
    await context.send("ℹ️ You are not currently signed in.");
  } else {
    await context.signout();
    await context.send("👋 You have been signed out successfully!");
  }
}

// Handler for profile command
async function handleProfile(context: any): Promise<void> {
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
      await context.send(profileInfo);
    } else {
      await context.send("❌ Could not retrieve your profile information.");
    }
  } catch (error: any) {
    if (error?.code === 'AuthenticationError' || error?.statusCode === 401) {
      context.logger?.error(`Authentication error: ${error}`);
      await context.send("🔐 Authentication failed. Please try signing in again.");
    } else {
      context.logger?.error(`Error getting profile: ${error}`);
      await context.send(`❌ Failed to get your profile: ${error?.message || error}`);
    }
  }
}

// Handler for default/unrecognized messages
async function handleDefault(context: any): Promise<void> {
  await context.send(
    "👋 **Hello! I'm a Teams Auth Quickstart and Graph bot.**\n\n" +
    "**Available commands:**\n\n" +
    "• **signin** - Sign in to your Microsoft account\n\n" +
    "• **signout** - Sign out\n\n" +
    "• **profile** - Show your profile information\n\n"
  );
}

// Handle error events
app.event('error', async (event: any) => {
  console.error(`Error occurred: ${event.error}`);
  if (event.context) {
    console.error(`Context: ${event.context}`);
  }
});

// Start the application
(async () => {
  await app.start();
  console.log(`\nBot started, app listening to`, process.env.PORT || process.env.port || 3978);
})();
