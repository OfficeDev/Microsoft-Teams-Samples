var express = require('express');
var router = express.Router();

router.post('/messages', require('./botController'));

module.exports = router;
