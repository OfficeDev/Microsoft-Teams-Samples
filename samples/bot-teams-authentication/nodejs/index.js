const app = require("./app");

// Start the application
app.start().catch(console.error);
console.log(`\nBot started, app listening to`, process.env.PORT || process.env.port || 3978);