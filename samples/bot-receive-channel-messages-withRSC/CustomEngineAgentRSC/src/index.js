// Import required packages
const express = require("express");

// This bot's adapter
const adapter = require("./adapter");

// This bot's main dialog.
const app = require("./app/app");

// Create express application.
const expressApp = express();
expressApp.use(express.json());

const server = expressApp.listen(process.env.port || process.env.PORT || 3978, () => {
  console.log(`\nBot Started, ${expressApp.name} listening to`, server.address());
});

// Listen for incoming requests.
expressApp.post("/api/messages", async (req, res) => {
  // Route received a request to adapter for processing
  await adapter.process(req, res, async (context) => {
    // Dispatch to application for routing
    await app.run(context);
  });
});
