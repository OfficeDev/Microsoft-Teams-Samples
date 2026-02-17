// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { stripMentionsText } from "@microsoft/teams.api";
import { App } from "@microsoft/teams.apps";
import { handleFileDownload, sendFileCard, handleFileConsent, processInlineImage } from "./handlers/attachments.js";
import { sendAdaptiveCardActions, sendToggleVisibilityCard } from "./handlers/adaptive-cards.js";

const app = new App();

// Handle bot installation and new members
app.on('conversationUpdate', async (context) => {
    const { activity } = context;
    const membersAdded = (activity as any).membersAdded || [];

    for (const member of membersAdded) {
        // Check if bot was added to the conversation
        if (member.id === activity.recipient.id) {
            await sendWelcomeMessage(context);
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
      await sendAdaptiveCardActions(context);
    } else if (normalizedText.includes('togglevisibility')) {
      await sendToggleVisibilityCard(context);
    } 
    // Handle file commands
    else if (normalizedText.includes('send file') || normalizedText.includes('file')) {
      await sendFileCard(context);
    } else {
      // Unrecognized command
      await sendWelcomeMessage(context);
    }
  } else if (attachment) {
    // Handle file attachments
    const imageRegex = /image\/.*/;
    if (attachment.contentType === 'application/vnd.microsoft.teams.file.download.info') {
      await handleFileDownload(attachment, context);
    } else if (imageRegex.test(attachment.contentType)) {
      await processInlineImage(context);
    } else {
      await sendFileCard(context);
    }
  } else {
    await sendWelcomeMessage(context);
  }
});

// Handle all invoke activities including file consent
app.on("invoke", async (context) => {
  await handleFileConsent(context);
});

// Sends welcome message
async function sendWelcomeMessage(context: any) {
  await context.send("Welcome to the Teams Bot Cards!");
}

app.start().catch(console.error);