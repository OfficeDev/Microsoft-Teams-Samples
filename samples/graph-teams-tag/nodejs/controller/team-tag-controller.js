// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { GraphHelper } = require("../helper/graph-helper");

/** Creates an new team tag. */
const createTeamTag = async (req, res) => {
    var teamId = req.query.teamId;
    var teamTag = req.body;

    try 
    {
        await GraphHelper.createTeamworkTagAsync(teamId, teamTag);
        res.status(201).send();
    }
    catch (ex) 
    {
        console.error(ex);
        res.status(500).send();
    }
}

/** List all the tags for the specified team */
const listTeamTagAsync = async (req, res) => {
    var teamId = req.query.teamId;

    try 
    {
       var teamTagsList = await GraphHelper.listTeamworkTagsAsync(teamId);
        res.status(200).send(teamTagsList.data.value);
    }
    catch (ex) 
    {
        console.error(ex);
        res.status(500).send();
    }
}

/** Update an existing tag. */
const updateTeamTagAsync = async (req, res) => {
    var teamId = req.query.teamId;
    var teamTag = req.body;

    try 
    {
       var teamTagsList = await GraphHelper.updateTeamworkTagAsync(teamId, teamTag);
        res.status(204).send();
    }
    catch (ex) 
    {
        console.error(ex);
        res.status(500).send();
    }
}

/** Gets the members of team tag. */
const getTeamworkTagMembersAsync = async (req, res) => {
    var teamId = req.query.teamId;
    var teamTagId = req.query.tagId;

    try 
    {
       var members = await GraphHelper.getTeamworkTagMembersAsync(teamId, teamTagId);
        res.status(200).send(members);
    }
    catch (ex) 
    {
        console.error(ex);
        res.status(500).send();
    }
}

module.exports = {
    createTeamTag,
    listTeamTagAsync,
    updateTeamTagAsync,
    getTeamworkTagMembersAsync
};