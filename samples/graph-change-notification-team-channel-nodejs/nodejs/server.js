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

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Parse application/json
app.use(express.json());

// Define route for the controller.
app.use('/api/changeNotification', require('./controller'))

const notification = async (req, res, next) => {
    let status;

    if (req.query && req.query.validationToken) {
        console.log("In controller", res);
        status = 200;
        res.send(req.query.validationToken);
    }
    else {
        let notification = req.body.value;

        try {
            var response = await DecryptionHelper.processEncryptedNotification(notification);
            res.status(202).send(response);
        }
        catch (ex) {
            console.error(ex);
            res.status(500).send();
        }
    }
}

// Listen for incoming requests.
app.post('/api/notifications', notification);

app.listen(3000, function () {
    console.log('app listening on port 3000!');
});