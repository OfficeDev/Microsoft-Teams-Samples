// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Bot AI - Teams SDK Sample

import { stripMentionsText } from "@microsoft/teams.api";
import { App } from "@microsoft/teams.apps";
import { DevtoolsPlugin } from "@microsoft/teams.dev";

const app = new App({
  plugins: [new DevtoolsPlugin()],
});

// Handle invoke activities (feedback button actions)
app.on("invoke", async (context) => {
  try {
    const activity = context.activity;
    
    switch (activity.name) {
      case "message/submitAction":
        const reaction = (activity as any).value?.actionValue?.reaction || "No reaction";
        const feedback = (activity as any).value?.actionValue?.feedback ? 
          JSON.parse((activity as any).value.actionValue.feedback).feedbackText : "No feedback";
        
        await context.send(`Provided reaction: ${reaction}<br> Feedback: ${feedback}`);
        break;
      
      default:
        await context.send(`Unknown invoke activity handled as default - ${activity.name}`);
        break;
    }
  } catch (err) {
    console.log(`Error in invoke activity: ${err}`);
    await context.send(`Invoke activity received - ${context.activity.name}`);
  }
});

// Handle message events
app.on("message", async ({ send, activity }) => {
  const text = stripMentionsText(activity).trim().toLowerCase();

  if (text.includes('label')) {
    await addAILabel(send);
  } else if (text.includes('sensitivity')) {
    await addSensitivityLabel(send);
  } else if (text.includes('feedback')) {
    await addFeedbackButtons(send);
  } else if (text.includes('citation')) {
    await addCitations(send);
  } else if (text.includes('aitext')) {
    await sendAIMessage(send);
  } else {
    await send("Welcome to Bot AI!");
  }
});

// Send a message with AI label
async function addAILabel(send: Function) {
  await send({
    type: 'message',
    text: "Hey I'm a friendly AI bot. This message is generated via AI",
    entities: [
      {
        type: "https://schema.org/Message",
        "@type": "Message",
        "@context": "https://schema.org",
        additionalType: ["AIGeneratedContent"],
      }
    ]
  });
}

// Send a message with sensitivity label
async function addSensitivityLabel(send: Function) {
  await send({
    type: 'message',
    text: "This is an example for sensitivity label that help users identify the confidentiality of a message",
    entities: [
      {
        type: "https://schema.org/Message",
        "@type": "Message",
        "@context": "https://schema.org",
        usageInfo: {
          "@type": "CreativeWork",
          description: "Please be mindful of sharing outside of your team",
          name: "Confidential \\ Contoso FTE",
        }
      }
    ]
  });
}

// Send a message with feedback buttons enabled
async function addFeedbackButtons(send: Function) {
  await send({
    type: 'message',
    text: "This is an example for Feedback buttons that helps to provide feedback for a bot message",
    channelData: {
      feedbackLoopEnabled: true
    },
  });
}

// Send a message with citations
async function addCitations(send: Function) {
  await send({
    type: 'message',
    text: "Hey I'm a friendly AI bot. This message is generated through AI [1]",
    entities: [
      {
        type: "https://schema.org/Message",
        "@type": "Message",
        "@context": "https://schema.org",
        citation: [
          {
            "@type": "Claim",
            position: 1,
            appearance: {
              "@type": "DigitalDocument",
              name: "AI bot",
              url: "https://example.com/claim-1",
              abstract: "Excerpt description",
              text: "{\"type\":\"AdaptiveCard\",\"$schema\":\"http://adaptivecards.io/schemas/adaptive-card.json\",\"version\":\"1.6\",\"body\":[{\"type\":\"TextBlock\",\"text\":\"Adaptive Card text\"}]}",
              keywords: ["keyword 1", "keyword 2", "keyword 3"],
              encodingFormat: "application/vnd.microsoft.card.adaptive",
              usageInfo: {
                "@type": "CreativeWork",
                name: "Confidential \\ Contoso FTE",
                description: "Only accessible to Contoso FTE",
              },
              image: {
                "@type": "ImageObject",
                name: "Microsoft Word"
              },
            },
          },
        ],
      },
    ],
  });
}

// Send a comprehensive AI message with all features
async function sendAIMessage(send: Function) {
  await send({
    type: 'message',
    text: "Hey I'm a friendly AI bot. This message is generated via AI [1]",
    channelData: {
      feedbackLoopEnabled: true,
    },
    entities: [
      {
        type: "https://schema.org/Message",
        "@type": "Message",
        "@context": "https://schema.org",
        usageInfo: {
          "@type": "CreativeWork",
          name: "Confidential \\ Contoso FTE",
          description: "Please be mindful of sharing outside of your team",
        },
        additionalType: ["AIGeneratedContent"],
        citation: [
          {
            "@type": "Claim",
            position: 1,
            appearance: {
              "@type": "DigitalDocument",
              name: "AI bot",
              url: "https://example.com/claim-1",
              abstract: "Excerpt description",
              encodingFormat: "application/vnd.microsoft.card.adaptive",
              keywords: ["keyword 1", "keyword 2", "keyword 3"],
              usageInfo: {
                "@type": "CreativeWork",
                name: "Confidential \\ Contoso FTE",
                description: "Only accessible to Contoso FTE",
              },
            },
          },
        ],
      },
    ],
  });
}

// Start the application
app.start().catch(console.error);
