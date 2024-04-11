// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const express = require('express');
const bodyparser = require('body-parser');
const path = require('path');
const cors = require('cors');

const server = express();

server.use(bodyparser.urlencoded({ extended: false }))
server.use(bodyparser.json())

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
  extended: true
}));

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

const port = process.env.port || process.env.PORT || 3000;
server.listen(port, function () {
  console.log(`app listening on port ${port}!`);
});

// Parse application/json
server.use(express.json());

// Define route for the controller.
server.use('/api/chat', require('./controller'))