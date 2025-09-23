// index.js is used to setup and configure your bot

// Import required packages
const express = require('express');
const path = require('path');

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require('botbuilder')
const { SearchApp } = require("./searchApp");
const config = require("./config");

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(process.env);

const adapter = new CloudAdapter(botFrameworkAuthentication);

adapter.onTurnError = async (context, error) => {
  // This check writes out errors to console log .vs. app insights.
  // NOTE: In production environment, you should consider logging this to Azure
  //       application insights. See https://aka.ms/bottelemetry for telemetry
  //       configuration instructions.
  console.error(`\n [onTurnError] unhandled error: ${error}`);

  // Send a message to the user
  await context.sendActivity(`The bot encountered an unhandled error:\n ${error.message}`);
  await context.sendActivity("To continue to run this bot, please fix the bot source code.");
};

// Create the bot that will handle incoming messages.
const searchApp = new SearchApp();

// Create HTTP server.
const server = express();
const port = process.env.port || process.env.PORT || 3978;
server.listen(port, () =>
    console.log(`\Bot/ME service listening at http://localhost:${port}`)
);

// Register middleware
server.use(express.json());
server.use("/images", express.static(path.resolve(__dirname, '../images')));

// Listen for incoming requests.
server.post('/api/messages', async (req, res) => {
    // Route received a request to adapter for processing
    await adapter.process(req, res, (context) => searchApp.run(context));
});

// Catch-all route should be last
server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});

// Gracefully shutdown HTTP server
["exit", "uncaughtException", "SIGINT", "SIGTERM", "SIGUSR1", "SIGUSR2"].forEach((event) => {
  process.on(event, () => {
    server.close();
  });
});