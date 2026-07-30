// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import express from 'express';
import botHandler from './botController';

const router = express.Router();

// Route to handle incoming messages
router.post('/messages', botHandler);

export default router;
