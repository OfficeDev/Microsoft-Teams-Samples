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
    let decryptedData;
    if (req.query && req.query.validationToken) {
        status = 200;
        res.send(req.query.validationToken);
    }

    else {
        let notification = req.body.value;
        try {
            decryptedData = await DecryptionHelper.processEncryptedNotification(notification);
            res.status(202).send();
        }
        catch (ex) {
            console.error(ex);
            res.status(500).send();
        }
    }

    /** Send Response to View */
    try {
        if (decryptedData) {
            notificationResponse = [{
                createdDate: decryptedData.createdDateTime,
                displayName: decryptedData.displayName,
                changeType: decryptedData.changeType
            }]

            var responseMessage = Promise.resolve(notificationResponse);

            responseMessage.then(function (result) {
                res.json(result);
                res.status();
            }, function (err) {
                console.log(err);
                res.json(err);
            });
        }
    }
    catch (e) {
        console.log("Error", e)
    }
});

app.listen(3000, function () {
    console.log('app listening on port 3000!');
});