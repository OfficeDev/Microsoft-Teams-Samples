// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const express = require("express");

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { CloudAdapter } = require("@microsoft/agents-hosting");
const { TeamsBot } = require("./bots/teamsBot");
const config = require("./config");

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new CloudAdapter({
  appId: config.botId,
  appPassword: config.botPassword,
});

adapter.onTurnError = async (context, error) => {
  // This check writes out errors to console log .vs. app insights.
  // NOTE: In production environment, you should consider logging this to Azure
  //       application insights. See https://aka.ms/bottelemetry for telemetry
  //       configuration instructions.
  console.error(`\n [onTurnError] unhandled error: ${error}`);

  // Uncomment below commented line for local debugging.
  // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);

  // Note: Since this Messaging Extension does not have the messageTeamMembers permission
  // in the manifest, the bot will not be allowed to message users.
};

// Create the bot that will handle incoming messages.
const bot = new TeamsBot();

// Create HTTP server.
const app = express();
app.use(express.json());

const port = process.env.port || process.env.PORT || 3978;
const server = app.listen(port, function () {
  console.log(`\nBot started, listening on port ${port}`);
});

// Listen for incoming requests.
app.post("/api/messages", async (req, res) => {
  await adapter.process(req, res, async (context) => {
    await bot.run(context);
  });
});

// Gracefully shutdown HTTP server
["exit", "uncaughtException", "SIGINT", "SIGTERM", "SIGUSR1", "SIGUSR2"].forEach((event) => {
  process.on(event, () => {
    server.close();
  });
});
