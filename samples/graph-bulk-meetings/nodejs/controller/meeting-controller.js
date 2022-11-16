// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { GraphHelper } = require("../helper/graph-helper");

/** Creates an new team tag. */
const createMeetingAsync = async (req, res) => {
    var userid = req.query.userId;
    var excelData = req.body;

    try {
        await GraphHelper.createMeetingAsync(userid, excelData);
        res.status(201).send();
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

/** List all the meetings for the specified user */
const listMeetingAsync = async (req, res) => {
    var userId = req.query.userId;

    try {
        var meetingList = await GraphHelper.listMeetingAsync(userId);
        res.status(200).send(meetingList.data.value);
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

module.exports = {
    createMeetingAsync,
    listMeetingAsync
};