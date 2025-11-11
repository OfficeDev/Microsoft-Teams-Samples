const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { TaskModuleUIConstants } = require('./models/taskmoduleuiconstants');
const { TaskModuleIds } = require('./models/taskmoduleids');
const { TaskModuleResponseFactory } = require('./models/taskmoduleresponsefactory');

// Create storage for conversation history
const storage = new LocalStorage();

// Create the app with storage and authentication
const app = new App({
  appId: config.MicrosoftAppId,
  appPassword: config.MicrosoftAppPassword,
  storage: storage,
});

const getConversationState = (conversationId) => {
  let state = storage.get(conversationId);
  if (!state) {
    state = { count: 0 };
    storage.set(conversationId, state);
  }
  return state;
};

/**
 * Handle new members added to the conversation
 */
app.on('membersAdded', async (context) => {
  const activity = context.activity;

  // Iterate over all new members added to the conversation
  for (const member of activity.membersAdded) {
    if (member.id !== activity.recipient.id) {
      await context.send({
        type: 'message',
        text: 'Welcome to Task Modules Bot! This bot demonstrates different types of task modules. Send any message to see the options.'
      });
      
      // Send the task module cards
      await sendTaskModuleCards(context);
    }
  }
});

/**
 * Handle task module fetch events
 */
app.on('invoke', async (context) => {
  const activity = context.activity;
  
  // Check if this is a task/fetch invoke
  if (activity.name === 'task/fetch') {
    const taskModuleRequest = activity.value;
    const cardTaskFetchValue = taskModuleRequest.data?.data || taskModuleRequest.data;

    const taskInfo = {};
    const baseUrl = process.env.BOT_ENDPOINT || 'https://localhost:3978';

    try {
      if (cardTaskFetchValue === TaskModuleIds.YouTube) {
        // For now, let's use an external YouTube URL directly
        taskInfo.url = taskInfo.fallbackUrl = 'https://www.youtube.com/embed/r9WQPSaLnaU';
        setTaskInfo(taskInfo, TaskModuleUIConstants.YouTube);
      } else if (cardTaskFetchValue === TaskModuleIds.CustomForm) {
        // Use an adaptive card for the custom form
        const card = createCustomFormCard();
        taskInfo.card = {
          contentType: 'application/vnd.microsoft.card.adaptive',
          content: card
        };
        setTaskInfo(taskInfo, TaskModuleUIConstants.CustomForm);
      } else if (cardTaskFetchValue === TaskModuleIds.AdaptiveCard) {
        const card = createAdaptiveCardAttachment();
        taskInfo.card = {
          contentType: 'application/vnd.microsoft.card.adaptive',
          content: card
        };
        setTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
      }
      
      return TaskModuleResponseFactory.toTaskModuleResponse(taskInfo);
    } catch (error) {
      return {
        task: {
          type: 'message',
          value: 'Sorry, there was an error processing your request.'
        }
      };
    }
  }
  
  // Check if this is a task/submit invoke
  if (activity.name === 'task/submit') {
    const taskModuleRequest = activity.value;
    
    // Echo the user's input back
    await context.send(`Task module submitted with data: ${JSON.stringify(taskModuleRequest.data)}`);

    // Return TaskModuleResponse
    return {
      task: {
        type: 'message',
        value: 'Thanks for submitting the form!'
      }
    };
  }
});

/**
 * Handle regular message events
 */
// Use a regex pattern to match all text messages
app.message(/./,  async (context) => {
  const activity = context.activity;
  const text = activity.text?.trim() || '';
  
  if (text === "/reset") {
    storage.delete(activity.conversation.id);
    await context.send("Ok I've deleted the current conversation state.");
    return;
  }

  if (text === "/count") {
    const state = getConversationState(activity.conversation.id);
    await context.send(`The count is ${state.count}`);
    return;
  }

  if (text === "/diag") {
    await context.send(JSON.stringify(activity));
    return;
  }

  if (text === "/state") {
    const state = getConversationState(activity.conversation.id);
    await context.send(JSON.stringify(state));
    return;
  }

  if (text === "/runtime") {
    const runtime = {
      nodeversion: process.version,
      sdkversion: "2.0.0", // Teams AI v2
    };
    await context.send(JSON.stringify(runtime));
    return;
  }

  // Default behavior: show task module cards
  const state = getConversationState(activity.conversation.id);
  state.count++;
  
  await context.send(`[${state.count}] you said: ${text}`);
  await sendTaskModuleCards(context);
});



/**
 * Send task module cards to the user
 * @param {Object} context The turn context.
 */
async function sendTaskModuleCards(context) {
  // Send HeroCard with task module options
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.hero',
      content: {
        title: 'Dialog (referred as task modules in TeamsJS v1.x) Invocation from Hero Card',
        buttons: [
          {
            type: 'invoke',
            title: TaskModuleUIConstants.YouTube.buttonTitle,
            value: {
              type: 'task/fetch',
              data: TaskModuleUIConstants.YouTube.id
            }
          },
          {
            type: 'invoke',
            title: TaskModuleUIConstants.CustomForm.buttonTitle,
            value: {
              type: 'task/fetch',
              data: TaskModuleUIConstants.CustomForm.id
            }
          },
          {
            type: 'invoke',
            title: TaskModuleUIConstants.AdaptiveCard.buttonTitle,
            value: {
              type: 'task/fetch',
              data: TaskModuleUIConstants.AdaptiveCard.id
            }
          }
        ]
      }
    }]
  });

  // Send AdaptiveCard with task module options
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.adaptive',
      content: {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        version: '1.4',
        type: 'AdaptiveCard',
        body: [
          {
            type: 'TextBlock',
            text: 'Dialog (referred as task modules in TeamsJS v1.x) Invocation from Adaptive Card',
            weight: 'bolder',
            size: 'medium'
          }
        ],
        actions: [
          {
            type: 'Action.Submit',
            title: TaskModuleUIConstants.YouTube.buttonTitle,
            data: { 
              msteams: { type: 'task/fetch' }, 
              data: TaskModuleUIConstants.YouTube.id 
            }
          },
          {
            type: 'Action.Submit',
            title: TaskModuleUIConstants.CustomForm.buttonTitle,
            data: { 
              msteams: { type: 'task/fetch' }, 
              data: TaskModuleUIConstants.CustomForm.id 
            }
          },
          {
            type: 'Action.Submit',
            title: TaskModuleUIConstants.AdaptiveCard.buttonTitle,
            data: { 
              msteams: { type: 'task/fetch' }, 
              data: TaskModuleUIConstants.AdaptiveCard.id 
            }
          }
        ]
      }
    }]
  });
}

/**
 * Sets task info properties
 * @param {Object} taskInfo The task info object.
 * @param {Object} uiSettings The UI settings object.
 */
function setTaskInfo(taskInfo, uiSettings) {
  taskInfo.height = uiSettings.height;
  taskInfo.width = uiSettings.width;
  taskInfo.title = uiSettings.title;
}

/**
 * Creates a custom form AdaptiveCard for user input
 * @returns {Object} The AdaptiveCard attachment.
 */
function createCustomFormCard() {
  return {
    version: '1.4',
    type: 'AdaptiveCard',
    body: [
      {
        type: 'TextBlock',
        text: 'Enter customer information:',
        weight: 'bolder',
        size: 'medium'
      },
      {
        type: 'Input.Text',
        id: 'name',
        label: 'Name',
        placeholder: 'First and last name'
      },
      {
        type: 'Input.Text',
        id: 'email',
        label: 'Email',
        placeholder: 'name@email.com',
        style: 'email'
      },
      {
        type: 'Input.Text',
        id: 'favoriteBook',
        label: 'Favorite Book',
        placeholder: 'Title of book'
      }
    ],
    actions: [
      {
        type: 'Action.Submit',
        title: 'Submit',
        data: {
          action: 'customForm'
        }
      }
    ]
  };
}

/**
 * Creates an AdaptiveCard attachment for user input
 * @returns {Object} The AdaptiveCard attachment.
 */
function createAdaptiveCardAttachment() {
  return {
    version: '1.0.0',
    type: 'AdaptiveCard',
    body: [
      {
        type: 'TextBlock',
        text: 'Enter Text Here'
      },
      {
        type: 'Input.Text',
        id: 'usertext',
        placeholder: 'add some text and submit',
        isMultiline: true
      }
    ],
    actions: [
      {
        type: 'Action.Submit',
        title: 'Submit'
      }
    ]
  };
}

module.exports = app;
