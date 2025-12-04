// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Import required packages
const express = require("express");
const path = require("path");

// Import required agent services.
// See Agent SDK documentation to learn more about agents.
const { CloudAdapter, loadPrevAuthConfigFromEnv } = require('@microsoft/agents-hosting');
const { ActionApp } = require("./actionApp");
const config = require("./config");

// Set environment variables for authentication
process.env.MicrosoftAppId = config.botId;
process.env.MicrosoftAppPassword = config.botPassword;

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const authConfig = loadPrevAuthConfigFromEnv();
const adapter = new CloudAdapter(authConfig);

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
const actionApp = new ActionApp();

// Create HTTP server.
const app = express();
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

const port = process.env.port || process.env.PORT || 3978;
app.listen(port, () => {
  console.log(`\nBot started, listening on port ${port}`);
});

// Listen for incoming requests.
app.post("/api/messages", async (req, res) => {
  await adapter.process(req, res, async (context) => {
    await actionApp.run(context);
  });
});

// Setup the static tab
app.get("/", (req, res) => {
  res.sendFile(path.join(__dirname, "views/hello.html"));
});

app.get("/tab", (req, res) => {
  res.sendFile(path.join(__dirname, "views/hello.html"));
});

// Task module tab
app.get("/taskModulePage", (req, res) => {
  res.sendFile(path.join(__dirname, "views/taskModule.html"));
});
