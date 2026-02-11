// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { stripMentionsText } from "@microsoft/teams.api";
import { App } from "@microsoft/teams.apps";
import { HandleFileDownload, SendFileCard, HandleFileConsent, ProcessInlineImage } from "./handlers/attachments.js";
import { SendAdaptiveCardActions, SendToggleVisibilityCard } from "./handlers/adaptive-cards.js";

const app = new App();

// Handle bot installation and new members
app.on('conversationUpdate', async (context) => {
    const { activity } = context;
    const membersAdded = (activity as any).membersAdded || [];

    for (const member of membersAdded) {
        // Check if bot was added to the conversation
        if (member.id === activity.recipient.id) {
            await SendWelcomeMessage(context);
        }
    }
});

// Handle message events
app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity) || "";
  const attachment = activity.attachments && activity.attachments[0];
  
  // Handle data submission from adaptive cards
  if (activity.value) {
    await context.send(`Data Submitted: ${JSON.stringify(activity.value)}`);
  } else if (text) {
    const normalizedText = text.trim().toLowerCase();
    
    // Handle card-related commands
    if (normalizedText.includes('card actions')) {
      await SendAdaptiveCardActions(context);
    } else if (normalizedText.includes('togglevisibility')) {
      await SendToggleVisibilityCard(context);
    } 
    // Handle file commands
    else if (normalizedText.includes('send file') || normalizedText.includes('file')) {
      await SendFileCard(context);
    } else {
      // Unrecognized command
      await SendWelcomeMessage(context);
    }
  } else if (attachment) {
    // Handle file attachments
    const imageRegex = /image\/.*/;
    if (attachment.contentType === 'application/vnd.microsoft.teams.file.download.info') {
      await HandleFileDownload(attachment, context);
    } else if (imageRegex.test(attachment.contentType)) {
      await ProcessInlineImage(context);
    } else {
      await SendFileCard(context);
    }
  } else {
    await SendWelcomeMessage(context);
  }
});

// Handle all invoke activities including file consent
app.on("invoke", async (context) => {
  await HandleFileConsent(context);
});

// Sends welcome message
async function SendWelcomeMessage(context: any) {
  await context.send("Welcome to the Teams Bot Cards!");
}

app.start().catch(console.error);