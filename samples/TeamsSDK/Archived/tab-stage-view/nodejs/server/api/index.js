const express = require('express');
const router = express.Router();

router.post('/messages', require('./botController'));

module.exports = router;