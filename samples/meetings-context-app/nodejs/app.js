// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const express = require('express');
const cors = require('cors');
const { stripMentionsText, MessageActivity } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");

// Create storage for conversation history
const storage = new LocalStorage();

const createTokenFactory = () => {
  return async (scope, tenantId) => {
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

// Configure authentication using TokenCredentials
const tokenCredentials = {
  clientId: process.env.CLIENT_ID || "",
  token: createTokenFactory(),
};

const credentialOptions =
  config.MicrosoftAppType === "UserAssignedMsi" ? { ...tokenCredentials } : undefined;

// Create the app with storage
const app = new App({
  ...credentialOptions,
  storage,
});

/**
 * Formats an object into HTML string for display
 * @param {object} obj Object to format
 * @returns {string} Formatted HTML string
 */
async function formatObject(obj) {
  let formattedString = "";
  Object.keys(obj).forEach((key) => {
    const block = `<b>${key}:</b> <br>`;
    let storeTemporaryFormattedString = "";
    if (typeof obj[key] === 'object' && obj[key] !== null) {
      Object.keys(obj[key]).forEach((secondKey) => {
        storeTemporaryFormattedString += ` <b> &nbsp;&nbsp;${secondKey}:</b> ${obj[key][secondKey]}<br/>`;
      });
      formattedString += block + storeTemporaryFormattedString;
      storeTemporaryFormattedString = "";
    }
  });
  return formattedString;
}

// Handle member added event
app.on("install.add", async ({ send, activity }) => {
      await send("Hello and welcome!");
      await send("Please use one of these two commands : " + `<b>Participant Context</b>` + " and " + `<b>Meeting Context</b> <br>` + "Thank you");
});


// Handle messages
app.on("message", async (client) => {
  const activity = client.activity;
  const text = (stripMentionsText(activity) || activity.text || "").toLowerCase();
  
  // Get meeting context data
  const meetingId = activity.channelData?.meeting?.id;
  const tenantId = activity.channelData?.tenant?.id;
  const participantId = activity.from.aadObjectId;

  if (text.includes("participant context")) {
    if (!meetingId || !participantId || !tenantId) {
      await client.send("This command only works in a meeting context.");
      return;
    }
    
    try {
      // The participant endpoint requires tenantId as a query parameter
      // Construct the full URL with tenantId
      const participantUrl = `${client.api.serviceUrl}/v1/meetings/${meetingId}/participants/${participantId}?tenantId=${tenantId}`;
      const response = await client.api.http.request({
        method: 'GET',
        url: participantUrl
      });
      const formattedString = await formatObject(response.data);
      await client.send(formattedString);
    } catch (error) {
      await client.send("Unable to retrieve participant context: " + error.message);
    }
  } else if (text.includes("meeting context")) {
    if (!meetingId) {
      await client.send("This command only works in a meeting context.");
      return;
    }
    
    try {
      const meetingInfo = await client.api.meetings.getById(meetingId);
      const formattedString = await formatObject(meetingInfo);
      await client.send(formattedString);
    } catch (error) {
      await client.send("Unable to retrieve meeting context: " + error.message);
    }
  } else {
    await client.send("Please use one of these two commands : " + `<b>Participant Context</b>` + " and " + `<b>Meeting Context</b> <br>` + "Thank you");
  }
});

// Setup custom API routes on Teams AI v2 Express instance
const setupApiRoutes = () => {
  const expressApp = app.http.express;

  if (expressApp) {
    // Configure CORS and body parsing
    expressApp.use(cors());
    expressApp.use(express.json());
    expressApp.use(express.urlencoded({ extended: true }));
  }
};


// Setup routes after app is created
setupApiRoutes();

module.exports = app;
