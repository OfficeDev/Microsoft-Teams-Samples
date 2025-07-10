// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const express = require('express');
const path = require('path');
const cors = require('cors');
const app = express();
const { DecryptionHelper } = require("./helper/decryption-helper");
const { GraphHelper } = require("./helper/graph-helper");

app.use(cors());
app.use(express.json());
app.use(express.urlencoded({
    extended: true
}));

var notificationList = [];

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Parse application/json
app.use(express.json());

// Define route for the controller.
app.use('/api/changeNotification', require('./controller'));

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
        debugger;
        if (JSON.stringify(notification) === '{}') {
            console.log("please Add/remove users from team or channel to get response data");
            res.send(notificationList);
        }
        else {
            debugger;
            notification = req.body.value;
            decryptedData = await DecryptionHelper.processEncryptedNotification(notification);
            
            let teamId, channelId;
            // If it's a channel deletion, check user access
            if (decryptedData.changeType === "deleted") {
                
                const resourceData = notification[0]?.resourceData;
                if (resourceData && resourceData['@odata.id']) {
                    const odataId = resourceData['@odata.id'];
                    const teamMatch = odataId.match(/teams\('([^']+)'\)/);
                    const channelMatch = odataId.match(/channels\('([^']+)'\)/);
                    
                    teamId = teamMatch ? teamMatch[1] : null;
                    channelId = channelMatch ? channelMatch[1] : null;
                }
            }                
              const notificationData = {
                createdDate: decryptedData.createdDateTime,
                displayName: decryptedData.displayName,
                changeType: decryptedData.changeType
            };
              // Only include hasUserAccess for deleted items
            if (decryptedData.changeType === "deleted") {
                const { userId, tenantId } = decryptedData;
                if (teamId && channelId && userId && tenantId) {
                    notificationData.hasUserAccess = await GraphHelper.checkUserChannelAccess(
                        teamId,
                        channelId,
                        userId,
                        tenantId
                    );
                }
            }
            
            notificationList.push(notificationData);
            console.log("Graph Api Notifications For Team and Channel");
            
            /** Send Respond to view **/
            res.send(notificationList);
        }
    }
});

app.listen(3000, function () {
    console.log('app listening on port 3000!');
});