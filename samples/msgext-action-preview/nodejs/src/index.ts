// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { ManagedIdentityCredential } from "@azure/identity";
import { cardAttachment, TokenCredentials } from "@microsoft/teams.api";
import { App } from "@microsoft/teams.apps";
import { ConsoleLogger } from "@microsoft/teams.common/logging";
import { DevtoolsPlugin } from "@microsoft/teams.dev";

import {
  createAdaptiveCardEditor,
  createAdaptiveCardAttachment,
  toSubmitExampleData,
  SubmitExampleData,
} from "./card";

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

// Configure authentication using TokenCredentials
const tokenCredentials: TokenCredentials = {
  clientId: process.env.CLIENT_ID || "",
  token: createTokenFactory(),
};

const credentialOptions =
  process.env.BOT_TYPE === "UserAssignedMsi" ? { ...tokenCredentials } : undefined;

const app = new App({
  ...credentialOptions,
  logger: new ConsoleLogger("msgext-action-preview", { level: "debug" }),
  plugins: [new DevtoolsPlugin()],
});

// Handle regular messages
app.on("message", async ({ send, activity }) => {
  if (activity.value) {
    // This was a message from the card.
    const obj = activity.value;
    const answer = obj.Answer;
    const choices = obj.Choices;
    await send(`${activity.from.name} answered '${answer}' and chose '${choices}'.`);
  } else {
    // This is a regular text message.
    await send('Hello from the TeamsMessagingExtensionsActionPreviewBot.');
  }
});

// Handle fetch task for messaging extension (when user clicks the command)
app.on("message.ext.open", async ({ activity }) => {
  const { commandId } = activity.value;
  
  if (commandId === "createWithPreview") {
    const adaptiveCard = createAdaptiveCardEditor();
    
    return {
      task: {
        type: "continue",
        value: {
          card: cardAttachment("adaptive", adaptiveCard),
          height: 450,
          title: "Task Module Fetch Example",
          width: 500,
        },
      },
    };
  }

  return { status: 400 };
});

// Handle submit action from task module
app.on("message.ext.submit", async ({ activity }) => {
  const { botMessagePreviewAction } = activity.value;
  
  // Check if this is a preview edit or send action
  if (botMessagePreviewAction === "edit") {
    // Handle preview edit (when user clicks Edit on the preview)
    const submitData = toSubmitExampleData(activity.value);

    // This is a preview edit call and so this time we want to re-create the adaptive card editor.
    const adaptiveCard = createAdaptiveCardEditor(
      submitData.Question,
      submitData.MultiSelect.toLowerCase() === 'true',
      submitData.Option1,
      submitData.Option2,
      submitData.Option3
    );

    return {
      task: {
        type: "continue",
        value: {
          card: cardAttachment("adaptive", adaptiveCard),
          height: 450,
          title: "Task Module Fetch Example",
          width: 500,
        },
      },
    };
  } else if (botMessagePreviewAction === "send") {
    // Handle preview send (when user clicks Send on the preview)
    const submitData = toSubmitExampleData(activity.value);

    // This is a send so we are done and we will create the adaptive card attachment.
    const adaptiveCard = createAdaptiveCardAttachment(submitData);

    return {
      composeExtension: {
        type: "result",
        attachmentLayout: "list",
        attachments: [cardAttachment("adaptive", adaptiveCard)],
      },
    };
  } else {
    // Initial submit from task module
    const submittedData = activity.value.data as SubmitExampleData;
    const adaptiveCard = createAdaptiveCardAttachment(submittedData);

    return {
      composeExtension: {
        type: "botMessagePreview",
        activityPreview: {
          type: "message",
          attachments: [cardAttachment("adaptive", adaptiveCard)],
        },
      },
    };
  }
});

// Handle card button clicked
app.on("message.ext.card-button", async ({ send, activity }) => {
  const reply = 'handleTeamsMessagingExtensionCardButtonClicked Value: ' + JSON.stringify(activity.value);
  await send(reply);
});

(async () => {
  await app.start();
})();
