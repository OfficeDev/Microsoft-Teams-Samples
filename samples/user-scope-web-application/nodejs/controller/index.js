// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const changeNotification = require('./notification-controller');
const express = require('express');
const router = express.Router();

// Routes for the API calls.
router.post('/channel/', changeNotification.createChannelAsync);
router.post('/team/', changeNotification.createTeamAsync);
router.post('/chat/',changeNotification.subscribeToSpecificChat);
router.post('/anychat/',changeNotification.subscribeToAnyChat);
router.post('/notifyOnUser/',changeNotification.notifyOnUserSpecificProperties);
router.post('/userLevelChats/',changeNotification.userLevelChats);
router.post('/userLevelMePath/',changeNotification.userLevelUsingMePath);
router.post('/UserLevelChatsUsingNotifyOnUser/',changeNotification.userLevelChatsUsingNotifyOnUserSpecificProperties);
router.post('/TeamsAppIsInstalled/',changeNotification.anyChatWhereTeamsAppIsInstalled);
router.get('/checkExistingSubsription/',changeNotification.checkExistingSubsription);
router.get('/getAllChats/',changeNotification.getAllChats);
router.get('/getAllMessages/',changeNotification.getAllMessageByChatId);

module.exports = router;