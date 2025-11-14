// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const path = require('path');
const express = require("express");

const cors = require('cors');

const ENV_FILE = path.join(__dirname, '..', '.env');
require('dotenv').config({ path: ENV_FILE });

const server = express();

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
  extended: true
}));

server.use('/api', require('./api'))

const port = process.env.port || process.env.PORT || 4001;
server.listen(port, () => {
  console.log(`Server listening on http://localhost:${port}`);
});