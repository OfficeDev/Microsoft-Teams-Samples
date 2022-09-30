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

var notificationResponse;

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Parse application/json
app.use(express.json());

// Define route for the controller.
app.use('/api/changeNotification', require('./controller'))

// Listen for incoming requests.
app.post('/api/messages', async (req, res) => {
    let status;

    if (req.query && req.query.validationToken) {
        console.log("In controller", res);
        status = 200;
        res.send(req.query.validationToken);
    }

    else {
        let response = null;
        response = req.body;

        try {
            if (response.channelData.channel) {
                notificationResponse = [{
                    createdDate: new Date().toString(),
                    displayName: response.channelData.channel.name,
                    changeType: response.channelData.eventType
                }]
            }
            else {
                notificationResponse = [{
                    createdDate: new Date().toString(),
                    displayName: response.channelData.team.name,
                    changeType: response.channelData.eventType
                }]
            }
        }
        catch (e) {
            console.log('Error', e)
        }
    }

    /** Send Response to View */
    try {
        if (notificationResponse) {
            var responseMessage = Promise.resolve(notificationResponse);
            responseMessage.then(function (result) {
                res.json(result);
                res.status(200).send();
            }, function (err) {
                console.log(err);
                res.json(err);
            });
        }
        else {
            res.status(500).send();
        }
    }
    catch (e) {
        console.log("Error", e)
    }
});

app.listen(3000, function () {
    console.log('app listening on port 3000!');
});