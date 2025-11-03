// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { GraphHelper } = require("../helper/graph-helper");

/** Creates Subscription for Channel */
const createChannelAsync = async (req, res) => {
    var teamId = req.query.teamId;
    var channelId = req.query.channelId;
    var pageId = "1";

    try {
        // Create both subscriptions
        await GraphHelper.createSubscription(teamId, pageId);
        await GraphHelper.createSharedWithTeamSubscription(teamId, channelId, pageId);

        res.status(202).json({
            message: "Subscriptions created successfully",
            teamId: teamId,
            channelId: channelId
        });
    } catch (ex) {
        console.error("Error in createChannelAsync:", ex);
        
        // Return specific error messages based on the error type
        if (ex.message.includes("app may not be enabled")) {
            return res.status(400).json({
                error: "App enablement issue",
                message: ex.message,
                teamId: teamId,
                channelId: channelId
            });
        }
        
        res.status(500).json({
            error: "Subscription creation failed",
            message: ex.message || "An unexpected error occurred while creating subscriptions",
            teamId: teamId,
            channelId: channelId
        });
    }
};

module.exports = {
    createChannelAsync
};