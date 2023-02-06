// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const express = require('express');
const path = require('path');
const cors = require('cors');
const app = express();

app.use(cors());
app.use(express.json());
app.use(express.urlencoded({
  extended: true
}));

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Parse application/json
app.use(express.json());

// Define route for the controller.
app.use('/api/teamtag', require('./controller'))

app.listen(3000, function () {
  console.log('app listening on port 3000!');
});