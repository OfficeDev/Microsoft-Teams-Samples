// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const meetingController = require('./meeting-controller');
const express = require('express');
const router = express.Router();

// Routes for the API calls.
router.post('/', meetingController.createMeetingAsync);
router.get('/list', meetingController.listMeetingAsync);
module.exports = router;