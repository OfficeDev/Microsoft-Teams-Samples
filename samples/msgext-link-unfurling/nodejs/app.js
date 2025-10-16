// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Import required packages
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");

// Initialize local storage for bot state
const storage = new LocalStorage();

/**
 * Creates a token factory for Azure Managed Identity authentication
 * This is used when the app is deployed to Azure with Managed Identity enabled
 * @returns {Function} Async function that returns an access token
 */
const createTokenFactory = () => {
  return async (scope, tenantId) => {
    // Create a managed identity credential using the client ID from environment variables
    const managedIdentityCredential = new ManagedIdentityCredential({
      clientId: process.env.CLIENT_ID,
    });
    
    // Ensure scopes is an array
    const scopes = Array.isArray(scope) ? scope : [scope];
    
    // Request an access token for the specified scope
    const tokenResponse = await managedIdentityCredential.getToken(scopes, {
      tenantId: tenantId,
    });

    return tokenResponse.token;
  };
};

// Configure token credentials for Managed Identity
const tokenCredentials = {
  clientId: process.env.CLIENT_ID || "",
  token: createTokenFactory(),
};

// Set credential options based on app type (only for Managed Identity deployments)
const credentialOptions =
  config.MicrosoftAppType === "UserAssignedMsi"
    ? { ...tokenCredentials }
    : undefined;

// Create the Teams app instance with credentials and storage
const app = new App({
  ...credentialOptions,
  storage,
});

/**
 * Event handler for when the app is installed
 * Sends a welcome message to the user explaining app functionality
 */
app.on("install.add", async ({ send }) => {
  const greeting = `
  Hi this app handles:<br>
    1. Link unfurling - creating preview cards when you paste URLs
  `;
  await send(greeting);
});

/**
 * Event handler for link unfurling (composeExtension/queryLink)
 * This is triggered when a user pastes a URL that matches the domains configured in the manifest
 * @param {Object} context - The context object containing activity information
 */
app.on("message.ext.query-link", async (context) => {
  // Extract the URL from the activity value
  const url = context.activity.value?.url;
  
  // Validate that a URL was provided
  if (!url) {
    console.log("[Link Unfurling] No URL found in activity");
    return;
  }

  /**
   * Create a thumbnail card for the preview
   * This is shown in the collapsed state before the user expands the card
   */
  const thumbnailCard = {
    contentType: "application/vnd.microsoft.card.thumbnail",
    content: {
      title: "Link Preview",
      subtitle: url,
      text: "Click to open the link",
      images: [
        {
          url: "https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png"
        }
      ],
      buttons: [
        {
          type: "openUrl",
          title: "Open Link",
          value: url
        }
      ]
    }
  };

  /**
   * Create an adaptive card for the expanded view
   * This is shown when the user clicks to expand the unfurled link
   */
  const adaptiveCard = {
    contentType: "application/vnd.microsoft.card.adaptive",
    content: {
      type: "AdaptiveCard",
      body: [
        {
          type: "TextBlock",
          text: "Unfurled Link",
          size: "Large",
          weight: "Bolder",
          color: "Accent",
          horizontalAlignment: "Center"
        },
        {
          type: "TextBlock",
          text: url,
          size: "Small",
          weight: "Lighter",
          color: "Good",
          wrap: true,
          horizontalAlignment: "Center"
        }
      ],
      $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
      version: "1.4"
    }
  };

  /**
   * Construct the response object for the message extension
   * Returns the adaptive card with a thumbnail preview
   */
  const response = {
    composeExtension: {
      type: "result",
      attachmentLayout: "list",
      attachments: [
        { 
          ...adaptiveCard,
          preview: thumbnailCard
        }
      ]
    }
  };
  
  // Return the response to Teams
  return response;
});

// Export the app instance
module.exports = app;
