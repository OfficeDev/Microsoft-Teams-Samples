const appController = require('./appController');

const express = require('express');
const router = express.Router();

router.post('/messages', require('./botController'));
router.post('/user/profile', appController.getUserProfile);
router.post('/user/photo', appController.getUserPhoto);

module.exports = router;
