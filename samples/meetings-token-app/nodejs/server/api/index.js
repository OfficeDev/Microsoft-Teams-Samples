const meetingController = require('./meetingController');
const express = require('express');
const router = express.Router();
router.use('/v1', require('./v1'));
router.post('/messages', require('./botController'));
router.post('/me/token', meetingController.getMeetingTokenAsync);
router.get('/meeting/summary', meetingController.getMeetingStatusAsync);
router.get('/me', meetingController.getUserInfoAsync);
router.post('/me/ack-token', meetingController.ackTokenAsync);
router.post('/user/skip', meetingController.skipTokenAsync);
router.post('/auth/token', meetingController.authToken);
router.post('/user/profile', meetingController.getUserProfile);
  
module.exports = router;