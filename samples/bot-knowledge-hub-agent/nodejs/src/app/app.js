const { ManagedIdentityCredential } = require("@azure/identity");
const { App } = require("@microsoft/teams.apps");
const { ChatPrompt } = require("@microsoft/teams.ai");
const { LocalStorage } = require("@microsoft/teams.common");
const { OpenAIChatModel } = require("@microsoft/teams.openai");
const { MessageActivity } = require('@microsoft/teams.api');
const fs = require('fs');
const path = require('path');
const config = require("../config");

// Create storage for conversation history
const storage = new LocalStorage();

// Load instructions from file on initialization
function loadInstructions() {
  const instructionsFilePath = path.join(__dirname, "instructions.txt");
  return fs.readFileSync(instructionsFilePath, 'utf-8').trim();
}

// Load instructions once at startup
const instructions = loadInstructions();

const createTokenFactory = () => {
  return async (scope, tenantId) => {
    const managedIdentityCredential = new ManagedIdentityCredential({
        clientId: process.env.CLIENT_ID
      });
    const scopes = Array.isArray(scope) ? scope : [scope];
    const tokenResponse = await managedIdentityCredential.getToken(scopes, {
      tenantId: tenantId
    });
   
    return tokenResponse.token;
  };
};

// Configure authentication using TokenCredentials
const tokenCredentials = {
  clientId: process.env.CLIENT_ID || '',
  token: createTokenFactory()
};

const credentialOptions = config.MicrosoftAppType === "UserAssignedMsi" ? { ...tokenCredentials } : undefined;

// Create the app with storage
const app = new App({
  ...credentialOptions,
  storage
});

// Handle incoming messages
app.on('message', async ({ send, stream, activity }) => {
  //Get conversation history
  const conversationKey = `${activity.conversation.id}/${activity.from.id}`;
  const messages = storage.get(conversationKey) || [];

  try {
    const prompt = new ChatPrompt({
      messages,
      instructions,
      model: new OpenAIChatModel({
        model: config.openAIModelName,
        apiKey: config.openAIKey
      })
    });

    // Personal chat only - use streaming response
    const response = await prompt.send(activity.text, {
      onChunk: (chunk) => {
        stream.emit(chunk);
      },
    });

    // Create message activity with citations
    const messageActivity = new MessageActivity(response.content).addAiGenerated();
    
    // Example cited documents (replace with your actual citation sources)
    const citedDocs = [
      { 
        title: "Teams AI Library", 
        content: "Sample content from first source",
        url: "https://learn.microsoft.com/en-us/microsoftteams/platform/teams-ai-library/"
      }
    ];
    // Add citations to the message
    let messageText = response.content;
    for (let i = 0; i < citedDocs.length; i++) {
      messageText += `[${i + 1}]`;
      messageActivity.addCitation(i + 1, {
        name: citedDocs[i].title,
        abstract: citedDocs[i].content,
        url: citedDocs[i].url
      });
    }
    
    // Update the message text with citation numbers
    messageActivity.text = messageText;
    
    // Add feedback functionality
    messageActivity.addFeedback('feedback', 'thumbs', 'How was this response?');
    
    // Send the final message with citations
    stream.emit(messageActivity);
    storage.set(conversationKey, messages);
  } catch (error) {
    console.error(error);
    await send("The agent encountered an error or bug.");
    await send("To continue to run this agent, please fix the agent source code.");
  }
});

app.on('message.submit.feedback', async ({ activity }) => {
  //add custom feedback process logic here
  console.log("Your feedback is " + JSON.stringify(activity.value));
});

module.exports = app;