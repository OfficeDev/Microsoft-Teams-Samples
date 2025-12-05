const { ManagedIdentityCredential } = require("@azure/identity");
const { App } = require("@microsoft/teams.apps");
const { ChatPrompt } = require("@microsoft/teams.ai");
const { OpenAIChatModel } = require("@microsoft/teams.openai");
const fs = require('fs');
const path = require('path');
const config = require("../config");

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

// Create the app with empty plugins array (will be added from index.js)
const app = new App({
  ...credentialOptions,
  plugins: []
});

// Handle messaging extension fetch task for sentiment analysis
app.on('message.ext.open', async ({ activity, send }) => {
  try {
    // Extract text from the message payload
    let textToAnalyze = activity.value?.messagePayload?.body?.content || '';
    
    // Remove HTML tags
    textToAnalyze = textToAnalyze.replace(/(<([^>]+)>)/ig, '');
    
    console.log("Text to analyze:", textToAnalyze);

    if (!textToAnalyze) {
      await send("No text found to analyze.");
      return;
    }

    // Create AI prompt for sentiment analysis
    const prompt = new ChatPrompt({
      messages: [],
      instructions,
      model: new OpenAIChatModel({
        model: config.openAIModelName,
        apiKey: config.openAIKey
      })
    });

    let sentimentResponse = "Not available";

    try {
      // Send the text to AI for sentiment analysis
      const response = await prompt.send(textToAnalyze);
      sentimentResponse = response.content.trim();
      console.log("Sentiment result:", sentimentResponse);
    } catch (error) {
      console.error("OpenAI error:", error.message);
      sentimentResponse = "Error analyzing sentiment";
    }

    // Determine the base URL
    const baseUrl = process.env.BASE_URL;
    
    // Create the task module URL with results
    // The tab is hosted at /tabs/sentimentModule per app.tab() registration
    const url = `${baseUrl}/tabs/sentimentModule?title=${encodeURIComponent(textToAnalyze)}&result=${encodeURIComponent(sentimentResponse)}`;

    // Return task module response with URL
    return {
      task: {
        type: 'continue',
        value: {
          width: 600,
          height: 400,
          title: 'Sentiment Analysis',
          url: url,
          fallbackUrl: url
        }
      }
    };
  } catch (error) {
    console.error("Error in fetchTask handler:", error);
    await send("An error occurred while processing the sentiment analysis.");
  }
});

// Handle welcome message when bot is added
app.on('conversationUpdate.membersAdded', async ({ activity, send }) => {
  const membersAdded = activity.membersAdded || [];
  for (const member of membersAdded) {
    if (member.id !== activity.recipient.id) {
      await send("Welcome to the Sentiment Analysis Sample. This example demonstrates how to analyze selected text and classify its sentiment as positive, negative, or neutral.");
    }
  }
});

module.exports = app;