/**
 * Main server entry point
 * Sets up and configures the bot for deployment
 */

const restify = require("restify");
const {
  CloudAdapter,
  ConfigurationServiceClientCredentialFactory,
  ConfigurationBotFrameworkAuthentication,
} = require("botbuilder");
const { SearchApp } = require("./searchApp");
const config = require("./config");

// Create adapter with single-tenant configuration
const credentialsFactory = new ConfigurationServiceClientCredentialFactory({
  MicrosoftAppId: config.botId,
  MicrosoftAppPassword: config.botPassword,
  MicrosoftAppType: "SingleTenant",
});

const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(
  {},
  credentialsFactory
);

const adapter = new CloudAdapter(botFrameworkAuthentication);

adapter.onTurnError = async (context, error) => {
  console.error(`[onTurnError] unhandled error: ${error}`);
  await context.sendActivity(`The bot encountered an unhandled error:\n ${error.message}`);
  await context.sendActivity("To continue to run this bot, please fix the bot source code.");
};

const searchApp = new SearchApp();

const server = restify.createServer();
server.use(restify.plugins.bodyParser());

server.listen(process.env.port || process.env.PORT || 3978, function () {
  console.log(`\nBot started, ${server.name} listening to ${server.url}`);
});

server.post("/api/messages", async (req, res) => {
  await adapter.process(req, res, async (context) => {
    await searchApp.run(context);
  });
});

// Gracefully shutdown HTTP server
["exit", "uncaughtException", "SIGINT", "SIGTERM", "SIGUSR1", "SIGUSR2"].forEach((event) => {
  process.on(event, () => {
    server.close();
  });
});
