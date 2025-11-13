const app = require("./app");

// Start the application
(async () => {
  // Start the Teams AI app
  await app.start();
  console.log(`Bot started, app listening to`, process.env.PORT || process.env.port || 3978);
})();