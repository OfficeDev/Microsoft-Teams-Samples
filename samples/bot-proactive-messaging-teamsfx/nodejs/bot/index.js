// index.js - setup and configure your bot

const restify = require("restify");
const {
    CloudAdapter,
    ConfigurationBotFrameworkAuthentication
} = require("botbuilder");
const { TeamsBot } = require("./teamsBot");

// Create bot framework authentication using environment variables
// Map TeamsFx environment variables to Bot Framework expected variables
const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication({
    MicrosoftAppId: process.env.BOT_ID,
    MicrosoftAppPassword: process.env.BOT_PASSWORD,
    MicrosoftAppTenantId: process.env.TEAMS_APP_TENANT_ID
});

// Create CloudAdapter
const adapter = new CloudAdapter(botFrameworkAuthentication);

// Global error handler
adapter.onTurnError = async (context, error) => {
    console.error(`\n [onTurnError] unhandled error: ${error}`);
    console.error(`Stack trace: ${error.stack}`);

    // Send a trace activity for Bot Framework Emulator
    await context.sendTraceActivity(
        "OnTurnError Trace",
        `${error}`,
        "https://www.botframework.com/schemas/error",
        "TurnError"
    );

    // Send a message to the user
    try {
        await context.sendActivity("The bot encountered an error or bug.");
        await context.sendActivity("To continue to run this bot, please fix the bot source code.");
    } catch (sendError) {
        console.error(`Error sending error message: ${sendError}`);
    }
};

// Conversation references store (used for proactive messaging)
const conversationReferences = {};

// Create the bot instance
const bot = new TeamsBot(conversationReferences);

// Create Restify server
const server = restify.createServer();
server.use(restify.plugins.bodyParser());

server.listen(process.env.port || process.env.PORT || 3978, () => {
    console.log(`\nBot started, ${server.name} listening to ${server.url}`);
    console.log(`Bot ID: ${process.env.BOT_ID}`);
    console.log(`Bot Password: ${process.env.BOT_PASSWORD ? '[SET]' : '[NOT SET]'}`);
    console.log(`Teams App Tenant ID: ${process.env.TEAMS_APP_TENANT_ID}`);
});

// Messages endpoint
server.post("/api/messages", async (req, res) => {
    console.log("Received message:", req.body);
    try {
        await adapter.process(req, res, (context) => bot.run(context));
    } catch (error) {
        console.error("Error processing message:", error);
        res.status(500).send("Internal Server Error");
    }
});

// Proactive messages endpoint
server.get("/api/notify", async (req, res) => {
    console.log("Sending proactive messages to stored conversation references...");
    console.log(JSON.stringify(conversationReferences, null, 2));

    try {
        for (const conversationReference of Object.values(conversationReferences)) {
            await adapter.continueConversationAsync(
                process.env.BOT_ID,
                conversationReference,
                async (context) => {
                    await context.sendActivity("Proactive hello from the bot!");
                }
            );
        }

        res.writeHead(200, { "Content-Type": "text/html" });
        res.end("<html><body><h1>Proactive messages have been sent.</h1></body></html>");
    } catch (error) {
        console.error("Error sending proactive messages:", error);
        res.writeHead(500, { "Content-Type": "application/json" });
        res.end(JSON.stringify({ code: "Internal", message: error.message }));
    }
});

// Graceful shutdown
["exit", "uncaughtException", "SIGINT", "SIGTERM", "SIGUSR1", "SIGUSR2"].forEach((event) => {
    process.on(event, () => {
        server.close(() => {
            console.log("HTTP server closed.");
            process.exit(0);
        });
    });
});
