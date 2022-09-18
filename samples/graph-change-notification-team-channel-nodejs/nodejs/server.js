// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const express = require('express');
const path = require('path');
const cors = require('cors');
const crypto = require("crypto");
const app = express();
const { DecryptionHelper } = require("../helper/decryption-helper");

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
app.use('/api/changeNotification', require('./controller'))

const decrypt = (text, dataKey) => {
  let iv = Buffer.from(text.iv, 'hex');
  let encryptedText = Buffer.from(text.encryptedData, 'hex');
  let decipher = crypto.createDecipheriv('aes-256-cbc', Buffer.from(dataKey), iv);
  let decrypted = decipher.update(encryptedText);
  decrypted = Buffer.concat([decrypted, decipher.final()]);
  return decrypted.toString();
}

const notification = async (req, res, next) => {
  let status;

  if (req.query && req.query.validationToken) {
    console.log("In controller", res);
    status = 200;
    res.send(req.query.validationToken);
  }
  else {
    clientStatesValid = false;
    let encryptedContent = req.body.value[0].encryptedContent.data;

    try {
      await DecryptionHelper.getDecryptedContent(encryptedContent);
      res.status(202).send();
    }
    catch (ex) {
      console.error(ex);
      res.status(500).send();
    }

    status = 202;
    res.send();
  }
}

// Listen for incoming requests.
app.post('/api/notifications', notification);

app.listen(3000, function () {
  console.log('app listening on port 3000!');
});