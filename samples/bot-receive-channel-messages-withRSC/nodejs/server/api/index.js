// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const express = require('express');
const router = express.Router();

// Route to handle incoming messages
router.post('/messages', require('./botController'));

module.exports = router;