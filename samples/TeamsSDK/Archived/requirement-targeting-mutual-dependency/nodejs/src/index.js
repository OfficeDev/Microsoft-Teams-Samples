// Import required packages
const express = require("express");
const http = require("http");
const https = require("https");
const fs = require("fs");
const path = require("path");

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const {
  CloudAdapter,
  ConfigurationServiceClientCredentialFactory,
  ConfigurationBotFrameworkAuthentication,
} = require("botbuilder");
const { ActionApp } = require("./actionApp");
const config = require("./config");

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const credentialsFactory = new ConfigurationServiceClientCredentialFactory({
  MicrosoftAppId: config.botId,
  MicrosoftAppPassword: config.botPassword,
  MicrosoftAppType: "SingleTenant",
  MicrosoftAppTenantId: config.botTenantId,
});

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

  // Send a message to the user
  await context.sendActivity(`The bot encountered an unhandled error:\n ${error.message}`);
  await context.sendActivity("To continue to run this bot, please fix the bot source code.");
};

// Create the bot that will handle incoming messages.
const actionApp = new ActionApp();

const app = express();
app.use(express.json());

// Listen for incoming requests.
app.post("/api/messages", async (req, res) => {
  await adapter.process(req, res, async (context) => {
    await actionApp.run(context);
  });
});

// Setup the static tab
app.get("/", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "hello.html"));
});

app.get("/tab", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "hello.html"));
});

// Task module tab
app.get("/taskModulePage", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "taskModule.html"));
});

const port = process.env.port || process.env.PORT || 3978;
const hasSsl = process.env.SSL_KEY_FILE && process.env.SSL_CRT_FILE;
const server = hasSsl
  ? https.createServer(
      {
        key: fs.readFileSync(process.env.SSL_KEY_FILE),
        cert: fs.readFileSync(process.env.SSL_CRT_FILE),
      },
      app
    )
  : http.createServer(app);

server.listen(port, () => {
  console.log(`\nBot started, listening on port ${port}`);
});
