// Import required packages
import * as restify from "restify";

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
import {
  CloudAdapter,
  ConfigurationServiceClientCredentialFactory,
  ConfigurationBotFrameworkAuthentication,
  TurnContext,
  ActivityTypes,
} from "botbuilder";

// This bot's main dialog.
import { SearchApp } from "./searchApp";
import config from "./config";

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const credentialsFactory = new ConfigurationServiceClientCredentialFactory({
  MicrosoftAppId: config.botId,
  MicrosoftAppPassword: config.botPassword,
  MicrosoftAppType: "MultiTenant",
});

const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(
  {},
  credentialsFactory
);

const adapter = new CloudAdapter(botFrameworkAuthentication);

// Catch-all for errors.
const onTurnErrorHandler = async (context: TurnContext, error: Error) => {
  // This check writes out errors to console log .vs. app insights.
  // NOTE: In production environment, you should consider logging this to Azure
  //       application insights.
  console.error(`\n [onTurnError] unhandled error: ${error}`);

  // Send a trace activity, which will be displayed in Bot Framework Emulator
  await context.sendTraceActivity(
    "OnTurnError Trace",
    `${error}`,
    "https://www.botframework.com/schemas/error",
    "TurnError"
  );

  // Send a message to the user
  await context.sendActivity(`The bot encountered unhandled error:\n ${error.message}`);
  await context.sendActivity("To continue to run this bot, please fix the bot source code.");
};

// Set the onTurnError for the singleton CloudAdapter.
adapter.onTurnError = onTurnErrorHandler;

// Create the bot that will handle incoming messages.
// const conversationReferences = {};
const continuationParameters: {} = {};
const searchApp = new SearchApp(async () => {
  console.log(
    `Handling continuation - ${JSON.stringify(continuationParameters)}`
  );
  for (const continuationParameter of Object.values(continuationParameters)) {
    const conversationReference = (continuationParameter as any)
      .conversationReference;

      await adapter.continueConversationAsync(
        (continuationParameter as any).claimsIdentity,
        conversationReference,
        (continuationParameter as any).oAuthScope,
        async (context) => {
          const continuationToken = (continuationParameter as any)
            .continuationToken;
          await context.sendActivities([
            {
              type: ActivityTypes.Message,
              text: "Continuing conversation from copilot...",
            },
            { type: ActivityTypes.Typing },
            { type: "delay", value: 1000 },
            {
              type: ActivityTypes.Message,
              text: `Fetching more details using the continuation token passed: ${continuationToken}`,
            },
            { type: ActivityTypes.Typing },
            { type: "delay", value: 4000 },
            {
              type: ActivityTypes.Message,
              text: `Handoff successful!`,
              attachments: [(continuationParameter as any).cardAttachment],
            },
          ]);
        }
      );
  }
}, continuationParameters /* conversationReferences */);

// Create HTTP server.
const server = restify.createServer();
server.use(restify.plugins.bodyParser());
server.listen(process.env.port || process.env.PORT || 3978, () => {
  console.log(`\nBot Started, ${server.name} listening to ${server.url}`);
});

// Listen for incoming requests.
server.post("/api/messages", async (req, res) => {
  await adapter.process(req, res, async (context) => {
    await searchApp.run(context);
  });
});
