// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const express = require('express');
const dotenv = require('dotenv');

// Load environment variables from .env file
const ENV_FILE = path.join(__dirname, '../.env');
dotenv.config({ path: ENV_FILE });

const server = express();

// Add this line so req.body is populated
server.use(express.json());

// Use the API routes
server.use('/api', require('./api'));

// Handle undefined routes
server.get('*', (req, res) => {
    res.status(404).json({ error: 'Route not found' });
});

// Set the port from environment variables or default to 3978
const port = process.env.PORT || 3978;

// Start the server
server.listen(port, () => {
    console.log(`Bot/ME service listening at http://localhost:${port}`);
});
