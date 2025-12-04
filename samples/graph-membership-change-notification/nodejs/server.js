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
var channelMembersList = new Map(); // Store member lists by teamId-channelId key

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Parse application/json
app.use(express.json());

// Define route for the controller.
app.use('/api/changeNotification', require('./controller'));

// Get channel members list
app.get('/api/members/:teamId/:channelId', async (req, res) => {
    try {
        const { teamId, channelId } = req.params;
        const memberKey = `${teamId}-${channelId}`;
        
        // Get fresh member list from Graph API
        const members = await GraphHelper.getChannelMembers(teamId, channelId);
        
        // Update local cache
        channelMembersList.set(memberKey, members);
        
        res.json({
            teamId,
            channelId,
            members: members,
            timestamp: new Date().toISOString()
        });
    } catch (error) {
        console.error('Error getting channel members:', error);
        res.status(500).json({
            error: 'Failed to get channel members',
            message: error.message
        });
    }
});

// Get cached member list
app.get('/api/members/cached/:teamId/:channelId', (req, res) => {
    const { teamId, channelId } = req.params;
    const memberKey = `${teamId}-${channelId}`;
    
    const cachedMembers = channelMembersList.get(memberKey) || [];
    
    res.json({
        teamId,
        channelId,
        members: cachedMembers,
        cached: true,
        timestamp: new Date().toISOString()
    });
});

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
            console.log("please Add/remove users from team or channel to get response data");
            res.send(notificationList);
        }
        else {
            notification = req.body.value;
            decryptedData = await DecryptionHelper.processEncryptedNotification(notification);
            
            let teamId, channelId;
            let shouldUpdateMemberList = true;
            let hasAccess = null;
            
            // Extract team and channel IDs from the notification
            if (notification[0]?.resourceData && notification[0].resourceData['@odata.id']) {
                const odataId = notification[0].resourceData['@odata.id'];
                const teamMatch = odataId.match(/teams\('([^']+)'\)/);
                const channelMatch = odataId.match(/channels\('([^']+)'\)/);
                
                teamId = teamMatch ? teamMatch[1] : null;
                channelId = channelMatch ? channelMatch[1] : null;
            }
            
            const notificationData = {
                createdDate: decryptedData.createdDateTime,
                displayName: decryptedData.displayName,
                changeType: decryptedData.changeType,
                teamId: teamId,
                channelId: channelId
            };
            
            // Handle different change types with conditional member list updates
            if (decryptedData.changeType === "deleted") {
                const { userId, tenantId } = decryptedData;
                if (teamId && channelId && userId && tenantId) {
                    hasAccess = await GraphHelper.checkUserChannelAccess(
                        teamId,
                        channelId,
                        userId,
                        tenantId
                    );
                    
                    notificationData.hasUserAccess = hasAccess;

                    shouldUpdateMemberList = !hasAccess;

                    if (hasAccess) {
                        console.log(`Skipping member list update for user ${userId} - user still has access`);
                    } else {
                        console.log(`User ${userId} no longer has access - updating member list`);
                    }
                }
            }
            
            // Handle shared/unshared events
            if (decryptedData.changeType === "created" && decryptedData.displayName) {
                // Shared event - update member list
                shouldUpdateMemberList = true;
                console.log(`Channel shared with team ${decryptedData.displayName} - updating member list`);
            }
            
            if (decryptedData.changeType === "deleted" && decryptedData.displayName && hasAccess) {
                // Unshared event - update member list
                shouldUpdateMemberList = true;
                console.log(`Channel unshared from team ${decryptedData.displayName} - updating member list`);
            }
            
            // Update member list conditionally
            if (shouldUpdateMemberList && teamId && channelId) {
                try {
                    const memberKey = `${teamId}-${channelId}`;
                    const updatedMembers = await GraphHelper.getChannelMembers(teamId, channelId);
                    channelMembersList.set(memberKey, updatedMembers);
                    
                    notificationData.memberListUpdated = true;
                    notificationData.currentMemberCount = updatedMembers.length;
                    
                    console.log(`Member list updated for ${memberKey}. Current count: ${updatedMembers.length}`);
                } catch (error) {
                    console.error('Error updating member list:', error);
                    notificationData.memberListUpdateError = error.message;
                }
            } else {
                notificationData.memberListUpdated = false;
                if (decryptedData.changeType === "deleted" && notificationData.hasUserAccess) {
                    notificationData.memberListSkipReason = "User still has access";
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