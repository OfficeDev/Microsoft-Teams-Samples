// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required pckages
const path = require('path');
const express = require('express');
const cors = require('cors');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Create HTTP server.
const server = express();
const port = process.env.port || process.env.PORT || 4001;
server.listen(port, () => 
    console.log(`Server listening on http://localhost:${port}`)
);

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));

server.use(express.static(path.resolve(__dirname, './client/build')));

// Listen for incoming requests.
server.use('/api', require('./server/api'));

server.get('*', (req, res) => {
    res.sendFile(path.resolve(__dirname, './client/build', 'index.html'));
});

