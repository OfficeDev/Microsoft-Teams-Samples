// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const app = require("./app");

// Start the application
(async () => {
  await app.start();
  console.log(`\nBot started, app listening to`, process.env.PORT || process.env.port || 3978);
})();
