// Load environment variables from .env.local
const path = require("path");
const envPath = path.join(__dirname, "env", ".env.local");
require("dotenv").config({ path: envPath });

const app = require("./app");
const config = require("./config");

// Start the application
(async () => {
  await app.start(config.Port);
  console.log(`\nBot started, app listening on port ${config.Port}`);
})();
