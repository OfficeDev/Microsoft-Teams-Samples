// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const express = require('express');
const path = require('path');
const cors = require('cors');
const app = express();
const { DecryptionHelper } = require("./helper/decryption-helper");

app.use(cors());
app.use(express.json());
app.use(express.urlencoded({
    extended: true
}));
var notificationResponse;

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Parse application/json
app.use(express.json());

// Define route for the controller.
app.use('/api/changeNotification', require('./controller'))

// Listen for incoming requests.
app.post('/api/notifications', async (req, res) => {
    let status;

    if (req.query && req.query.validationToken) {
        console.log("In controller", res);
        status = 200;
        res.send(req.query.validationToken);
    }
    else {
        let notification = req.body.value;

        try {
            notificationResponse = await DecryptionHelper.processEncryptedNotification(notification);
            res.status(202).send();
        }
        catch (ex) {
            console.error(ex);
            res.status(500).send();
        }
    }

    // send decrypted Response to view
    var responseMessage = Promise.resolve(notificationResponse);
    responseMessage.then(function (result) {
        res.json(result);
    }, function (err) {
        console.log(err);
        res.json(err);
    });
});

app.listen(3978, function () {
    console.log('app listening on port 3978!');
});