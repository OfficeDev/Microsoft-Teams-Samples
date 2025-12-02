const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ProactiveBot } = require("./bots/proactiveBot");

// Create storage for conversation history
const storage = new LocalStorage();

// Create the app with storage and authentication
// For MultiTenant bots, Teams SDK will use CLIENT_ID and CLIENT_PASSWORD from environment
const app = new App({
  storage,
});

// Initialize the proactive bot
const bot = new ProactiveBot(app);

module.exports = app;
