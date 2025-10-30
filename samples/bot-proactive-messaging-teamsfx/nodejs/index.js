const app = require("./app");

// Start the application
(async () => {
  // Start the Teams AI app
  await app.start();
  console.log(`Bot started, app listening to`, process.env.PORT || process.env.port || 3978);
  
  // Setup custom routes after the app has started
  const routesSetup = app.setupCustomRoutes();
  console.log(`Custom routes setup: ${routesSetup ? 'Success' : 'Failed'}`);
})();