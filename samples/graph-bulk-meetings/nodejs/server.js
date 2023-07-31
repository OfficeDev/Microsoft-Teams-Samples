// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const express = require('express');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Create HTTP server.
const server = express();

// Define route for the controller.
server.use('/api/meeting', require('./controller'))

const port = process.env.port || process.env.PORT || 3000;
server.listen(port, () => 
    console.log(`\Bot/ME service listening at http://localhost:${port}`)
);

