// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { GraphHelper } = require("../helper/graph-helper");

/** Creates Subscription for Channel */
const createChannelAsync = async (req, res) => {
    var teamId = req.query.teamId;
    var pageId = "1";

    try {
        await GraphHelper.createSubscription(teamId, pageId);
        res.status(202).send();
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

/** Creates Subscription for Team */
const createTeamAsync = async (req, res) => {
    var teamId = req.query.teamId;
    var pageId = "2";

    try {
        var result = await GraphHelper.createSubscription(teamId, pageId);
        res.status(202).send(result);
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

module.exports = {
    createChannelAsync,
    createTeamAsync
};