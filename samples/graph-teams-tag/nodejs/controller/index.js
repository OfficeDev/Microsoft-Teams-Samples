// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const teamTagController = require('./team-tag-controller');
const express = require('express');
const router = express.Router();

// Routes for the API calls.
router.post('/', teamTagController.createTeamTag);
router.get('/appData', teamTagController.getAppDetails);
router.get('/list', teamTagController.listTeamTagAsync);
router.patch('/update', teamTagController.updateTeamTagAsync);
router.delete('/delete', teamTagController.deleteTagAsync);
router.get('/members', teamTagController.getTeamworkTagMembersAsync);
  
module.exports = router;