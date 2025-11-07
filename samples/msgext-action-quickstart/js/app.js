const { stripMentionsText } = require("@microsoft/teams.api");
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

  // Echo behavior with conversation state
  const state = getConversationState(activity.conversation.id);
  state.count++;
  await context.send(`[${state.count}] you said: ${text}`);
});

// Messaging Extension handlers
app.on("activity", async (context, state) => {
  const activity = context.activity;
  
  // Handle messaging extension submit action
  if (activity.type === 'invoke' && activity.name === 'composeExtension/submitAction') {
    const action = activity.value;
    
    switch (action.commandId) {
      case 'createCard':
        return createCardCommand(context, action);
      case 'shareMessage':  
        return shareMessageCommand(context, action);
      default:
        throw new Error('NotImplemented');
    }
  }
  
  // Handle messaging extension link query
  if (activity.type === 'invoke' && activity.name === 'composeExtension/queryLink') {
    const query = activity.value;
    const attachment = {
      contentType: 'application/vnd.microsoft.card.thumbnail',
      content: {
        title: 'Thumbnail Card',
        subtitle: query.url,
        images: [{ 
          url: 'https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png' 
        }]
      }
    };

    const result = {
      composeExtension: {
        type: 'result',
        attachmentLayout: 'list',
        attachments: [attachment]
      }
    };
    
    return result;
  }
});

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

module.exports = app;
