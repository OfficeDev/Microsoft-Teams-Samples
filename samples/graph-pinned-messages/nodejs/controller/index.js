// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const pinnedMessageController = require('./pin-message-controller');
const express = require('express');
const router = express.Router();

// Routes for the API calls.
router.get('/getGraphAccessToken', pinnedMessageController.getGraphAccessToken);
router.get('/pinMessage', pinnedMessageController.pinNewMessage);
router.get('/unpinMessage', pinnedMessageController.unpinMessage);
  
module.exports = router;