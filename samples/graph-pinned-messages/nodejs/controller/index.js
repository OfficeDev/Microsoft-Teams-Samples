// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const teamTagController = require('./pin-message-controller');
const express = require('express');
const router = express.Router();

// Routes for the API calls.
router.get('/getGraphAccessToken', teamTagController.getGraphAccessToken);
router.get('/pinMessage', teamTagController.pinNewMessage);
router.get('/unpinMessage', teamTagController.unpinMessage);
  
module.exports = router;