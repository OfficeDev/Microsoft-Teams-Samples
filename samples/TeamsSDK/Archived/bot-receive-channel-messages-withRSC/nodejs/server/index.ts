// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import path from 'path';
import express from 'express';
import dotenv from 'dotenv';
import apiRouter from './api';

// Load environment variables from .env file
const ENV_FILE = path.join(__dirname, '../.env');
dotenv.config({ path: ENV_FILE });

const server = express();

server.use(express.json());

// Use the API routes
server.use('/api', apiRouter);

// Set the port from environment variables or default to 3978
const port = process.env.PORT || 3978;

// Start the server
server.listen(port, () => {
    console.log(`Bot/ME service listening at http://localhost:${port}`);
});
