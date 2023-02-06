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

var notificationResponse = [];
var notificationList = [];

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Parse application/json
app.use(express.json());

// Define route for the controller.
app.use('/api/changeNotification', require('./controller'))

// Listen for incoming requests.
app.post('/api/notifications', async (req, res) => {
    let status;
    var decryptedData = [];

    if (req.query && req.query.validationToken) {
        status = 200;
        res.send(req.query.validationToken);
    }
    else {
        var notification = req.body;

        if (JSON.stringify(notification) === '{}') {
            console.log("please create/update/delete channel/team to get response data");
            res.send(notificationList);

        }
        else {
            notification = req.body.value;
            decryptedData = await DecryptionHelper.processEncryptedNotification(notification);
            notificationResponse.push(decryptedData);

            notificationResponse.forEach(element => {
                notificationList.push({
                    createdDate: element.createdDateTime,
                    displayName: element.displayName,
                    changeType: element.changeType
                })
            });

            console.log("Graph Api Notifications For Team and Channel");

            /** Send Respond to view **/
            res.send(notificationList);
        }
    }
});

app.listen(3000, function () {
    console.log('app listening on port 3000!');
});