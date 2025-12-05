const app = require("./app");

// Start the application
(async () => {
  await app.start();
  console.log(`Bot started, app listening on port ${process.env.PORT || process.env.port || 3978}`);
})();
