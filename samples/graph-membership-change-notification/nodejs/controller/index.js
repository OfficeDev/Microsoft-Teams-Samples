// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const changeNotification = require('./notification-controller');
const express = require('express');
const router = express.Router();

// Routes for the API calls.
router.post('/', changeNotification.createChannelAsync);
module.exports = router;