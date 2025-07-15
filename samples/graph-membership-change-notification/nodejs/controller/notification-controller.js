// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { GraphHelper } = require("../helper/graph-helper");

/** Creates Subscription for Channel */
const createChannelAsync = async (req, res) => {
    var teamId = req.query.teamId;
    var channelId = req.query.channelId;
    var pageId = "1";

    try {
        debugger;
        await GraphHelper.createSubscription(teamId, pageId);
        await GraphHelper.createSharedWithTeamSubscription(teamId, channelId, pageId);
        res.status(202).send();
    } catch (ex) {
        debugger;
        console.error(ex);
        res.status(500).send();
    }
};

module.exports = {
    createChannelAsync
};