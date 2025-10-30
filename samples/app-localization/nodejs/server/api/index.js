// Import the express module and router to handle routing for the bot API
const express = require('express');
const router = express.Router();

/**
 * The `/v1` route serves versioned API requests.
 * It is dynamically requiring the routes for version 1 from the `v1` module.
 * The specific versioned routes are handled in `./v1`.
 */
router.use('/v1', require('./v1'));

/**
 * The `/messages` route handles POST requests related to bot messages.
 * It routes all POST requests to `botController` for further processing.
 */
router.post('/messages', require('./botController'));

// Export the configured router so it can be used in the main app.
module.exports = router;
