// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const app = require('./app');

// Start the Teams AI v2 app (creates server with /api/messages endpoint automatically)
(async () => {
    await app.start();
    console.log(`\nBot started, app listening on port ${process.env.PORT || process.env.port || 3978}`);
})();
