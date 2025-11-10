const path = require('path');
const app = require("./app/app");

// Register the tab to host the sentiment analysis results page
app.tab('sentimentModule', path.join(__dirname, 'client'));

// Start the application
(async () => {
  await app.start(3978); 
  console.log(`app listening on port ${3978}`)
})();
