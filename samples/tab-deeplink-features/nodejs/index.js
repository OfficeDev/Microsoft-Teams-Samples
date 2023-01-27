// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const dotenv = require('dotenv');
const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });
const express = require('express');

// Create HTTP server
const server = express();
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));

server.listen(process.env.port || process.env.PORT || 3978, () => {
    console.log(`\n${ server.name } listening to :: ${ process.env.PORT }`);
});

// Static Middleware
server.use(express.static(path.join(__dirname, './pages')));
