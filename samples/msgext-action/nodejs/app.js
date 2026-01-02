const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const express = require('express');
const path = require('path');

// Set base URL from environment
const baseUrl = process.env.BOT_ENDPOINT || process.env.BaseUrl || `https://localhost:${process.env.PORT || 3978}`;

// Create storage for conversation history
const storage = new LocalStorage();

// Create the app using the EXACT same pattern as the working reference
const app = new App({
  appId: config.MicrosoftAppId,
  appPassword: config.MicrosoftAppPassword,
  storage,
});



const getConversationState = (conversationId) => {
  let state = storage.get(conversationId);
  if (!state) {
    state = { count: 0 };
    storage.set(conversationId, state);
  }
  return state;
};

// Message handlers
app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity);

  if (text === '/test') {
    await context.send('Bot is working!');
    return;
  }

  const state = getConversationState(activity.conversation.id);
  state.count++;
  await context.send(`[${state.count}] you said: ${text}`);
});

// Original Teams AI v2 activity handler removed - now using custom express server with handleMessagingExtension

function createCardCommand(context, action) {
  const data = action.data;
  
  const heroCard = {
    contentType: 'application/vnd.microsoft.card.hero',
    content: {
      title: data.title,
      subtitle: data.subTitle,
      text: data.text
    }
  };
  
  const attachment = { 
    contentType: heroCard.contentType, 
    content: heroCard.content, 
    preview: heroCard 
  };

  return {
    composeExtension: {
      type: 'result',
      attachmentLayout: 'list',
      attachments: [attachment]
    }
  };
}

function shareMessageCommand(context, action) {
  let userName = 'unknown';
  if (action.messagePayload?.from?.user?.displayName) {
    userName = action.messagePayload.from.user.displayName;
  }

  let images = [];
  if (action.data.includeImage === 'true') {
    images = [{ url: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU' }];
  }

  const heroCard = {
    contentType: 'application/vnd.microsoft.card.hero',
    content: {
      title: `${userName} originally sent this message:`,
      text: action.messagePayload.body.content,
      images: images
    }
  };

  if (action.messagePayload?.attachments?.length > 0) {
    heroCard.content.subtitle = `(${action.messagePayload.attachments.length} Attachments not included)`;
  }

  const attachment = { 
    contentType: heroCard.contentType, 
    content: heroCard.content, 
    preview: heroCard 
  };

  return {
    composeExtension: {
      type: 'result',
      attachmentLayout: 'list',
      attachments: [attachment]
    }
  };
}

async function getSingleMember(context) {
  try {
    const fromUser = context.activity.from;
    
    if (fromUser && fromUser.name) {
      return fromUser.name;
    }
    
    if (fromUser && fromUser.id) {
      return `User ${fromUser.id.split('-')[0]}...`;
    }
    
    return 'Unknown User';
  } catch (e) {
    console.error('Error getting member:', e);
    return 'Error getting user info';
  }
}

/**
 * Creates a just-in-time installation card (Bot Framework format)
 */
function getJustInTimeCardAttachment() {
  return {
    contentType: 'application/vnd.microsoft.card.adaptive',
    content: {
      type: 'AdaptiveCard',
      version: '1.0',
      body: [
        {
          type: 'TextBlock',
          text: 'Looks like you have not used Action Messaging Extension app in this team/chat. Please click **Continue** to add this app.',
          wrap: true
        }
      ],
      actions: [
        {
          type: 'Action.Submit',
          title: 'Continue',
          data: { 
            msteams: { 
              justInTimeInstall: true 
            } 
          }
        }
      ]
    }
  };
}

/**
 * Creates an adaptive card attachment for roster info (Bot Framework format)
 */
function getAdaptiveCardAttachment() {
  return {
    contentType: 'application/vnd.microsoft.card.adaptive',
    content: {
      type: 'AdaptiveCard',
      version: '1.0',
      body: [
        {
          type: 'TextBlock',
          text: 'This app is installed in this conversation. You can now use it to do some great stuff!!!',
          wrap: true,
          isSubtle: false
        }
      ],
      actions: [
        {
          type: 'Action.Submit',
          title: 'Close'
        }
      ]
    }
  };
}

function webViewResponse(action) {
  const data = action.data;
  
  const heroCard = {
    contentType: 'application/vnd.microsoft.card.hero',
    content: {
      title: `ID: ${data.EmpId}`,
      subtitle: `Name: ${data.EmpName}`,
      text: `E-Mail: ${data.EmpEmail}`
    }
  };
  
  const attachment = { 
    contentType: heroCard.contentType, 
    content: heroCard.content, 
    preview: heroCard 
  };

  return {
    composeExtension: {
      type: 'result',
      attachmentLayout: 'list',
      attachments: [attachment]
    }
  };
}

function empDetails() {
  return {
    task: {
      type: 'continue',
      value: {
        url: `${baseUrl}/customForm`,
        height: 400,
        title: 'Employee Details',
        width: 300
      }
    }
  };
}

function dateTimeInfo() {
  return {
    task: {
      type: 'continue',
      value: {
        url: `${baseUrl}/staticPage`,
        height: 400,
        title: 'Static HTML',
        width: 300
      }
    }
  };
}

async function handleMessagingExtension(context) {
  const activity = context.activity;
  
  if (activity.type === 'invoke' && activity.name === 'composeExtension/submitAction') {
    const action = activity.value;
    
    switch (action.commandId) {
      case 'createCard':
        return createCardCommand(context, action);
      case 'shareMessage':  
        return shareMessageCommand(context, action);
      case 'webView':
        return webViewResponse(action);
      case 'FetchRoster':
        return null;
      default:
        throw new Error(`NotImplemented: Unknown command '${action.commandId}'`);
    }
  }

  if (activity.type === 'invoke' && activity.name === 'composeExtension/fetchTask') {
    const action = activity.value;
    
    switch (action.commandId) {
      case 'webView':
        return empDetails();
      case 'Static HTML':
        return dateTimeInfo();
      default:
        try {
          const member = await getSingleMember(context);
          const adaptiveCard = getAdaptiveCardAttachment();
          
          return {
            task: {
              type: 'continue',
              value: {
                card: adaptiveCard,
                height: 400,
                title: `Hello ${member}`,
                width: 300
              }
            }
          };
        } catch (e) {
          if (e.code === 'BotNotInConversationRoster') {
            return {
              task: {
                type: 'continue',
                value: {
                  card: getJustInTimeCardAttachment(),
                  height: 400,
                  title: 'Adaptive Card - App Installation',
                  width: 300
                }
              }
            };
          }
          throw e;
        }
    }
  }
  
  return undefined;
}

module.exports = { app, handleMessagingExtension };