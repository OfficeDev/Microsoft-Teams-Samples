// index.js is used to setup and configure your bot

// Import required packages
const restify = require("restify");

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { BotFrameworkAdapter } = require("botbuilder");
const { TeamsBot } = require("./teamsBot");

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const adapter = new BotFrameworkAdapter({
  appId: process.env.BOT_ID,
  appPassword: process.env.BOT_PASSWORD,
});

// Handle errors encountered by the bot.
adapter.onTurnError = async (context, error) => {
  console.error(`\n [onTurnError] unhandled error: ${error}`);
  // Uncomment below line for local debugging.
  // await context.sendActivity(`Sorry, it looks like something went wrong. Exception Caught: ${error}`);
};

// Create the bot that will handle incoming messages.
const conversationReferences = {};
const bot = new TeamsBot(conversationReferences);

// Create HTTP server.
const server = restify.createServer();
server.use(restify.plugins.bodyParser());

server.listen(process.env.port || process.env.PORT || 3978, () => {
  console.log(`\nBot started, ${server.name} listening to ${server.url}`);
});

// Listen for incoming requests.
server.post("/api/messages", async (req, res) => {
  await adapter.processActivity(req, res, async (context) => {
    await bot.run(context);
  });
});

// Listen for incoming notifications and send proactive messages to users.
server.get('/api/notify', async (req, res) => {
  console.log(JSON.stringify(conversationReferences));
  for (const conversationReference of Object.values(conversationReferences)) {
    await adapter.continueConversation(conversationReference, async (context) => {
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