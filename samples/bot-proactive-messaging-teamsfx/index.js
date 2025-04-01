// index.js is used to setup and configure your bot

// Import required packages
const express = require("express");

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const {
  CloudAdapter,
  ConfigurationServiceClientCredentialFactory,
  ConfigurationBotFrameworkAuthentication,
} = require("botbuilder");
const { TeamsBot } = require("./teamsBot");
const config = require("./config");

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const credentialsFactory = new ConfigurationServiceClientCredentialFactory(config);

const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(
  {},
  credentialsFactory
);

const adapter = new CloudAdapter(botFrameworkAuthentication);

adapter.onTurnError = async (context, error) => {
  // This check writes out errors to console log .vs. app insights.
  // NOTE: In production environment, you should consider logging this to Azure
  //       application insights. See https://aka.ms/bottelemetry for telemetry
  //       configuration instructions.
  console.error(`\n [onTurnError] unhandled error: ${error}`);

  // Only send error message for user messages, not for other message types so the bot doesn't spam a channel or chat.
  if (context.activity.type === "message") {
    // Send a message to the user
    await context.sendActivity(`The bot encountered an unhandled error:\n ${error.message}`);
    await context.sendActivity("To continue to run this bot, please fix the bot source code.");
  }
};

// Create the bot that will handle incoming messages.
const conversationReferences = {};
const bot = new TeamsBot(conversationReferences);

// Create express application.
const expressApp = express();
expressApp.use(express.json());

const server = expressApp.listen(process.env.port || process.env.PORT || 3978, () => {
  console.log(`\nBot Started, ${expressApp.name} listening to`, server.address());
});

// Listen for incoming requests.
expressApp.post("/api/messages", async (req, res) => {
  await adapter.process(req, res, async (context) => {
    await bot.run(context);
  });
});

// Listen for incoming notifications and send proactive messages to users.
expressApp.get('/api/notify', async (req, res) => {
  console.log(JSON.stringify(conversationReferences));
  for (const conversationReference of Object.values(conversationReferences)) {

    await adapter.continueConversationAsync(config.MicrosoftAppId,conversationReference, async (context) => {
      await context.sendActivity('proactive hello');
    });
  }

  res.setHeader('Content-Type', 'text/html');
  res.writeHead(200);
  res.write('<html><body><h1>Proactive messages have been sent.</h1></body></html>');
  res.end();
});

// Gracefully shutdown HTTP server
["exit", "uncaughtException", "SIGINT", "SIGTERM", "SIGUSR1", "SIGUSR2"].forEach((event) => {
  process.on(event, () => {
    server.close();
  });
});
