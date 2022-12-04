// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { polyfills } = require('isomorphic-fetch');
const msal = require('@azure/msal-node');
const { SimpleGraphClient } = require('../simpleGraphClient');

var scopes = ["https://graph.microsoft.com/User.Read"];

const msalClient = new msal.ConfidentialClientApplication({
    auth: {
      clientId: process.env.MicrosoftAppId,
      clientSecret: process.env.MicrosoftAppPassword
    }
  });

/** Creates an new team tag. */
const createTeamTag = async (req, res) => {
    var teamId = req.query.teamId;
    var teamTag = req.body;
    var tid = process.env.MicrosoftAppTenantId;
    var token = req.query.ssoToken;
    var accesstoken = await getDelegateAccessToken(tid, token);

    const client = new SimpleGraphClient(accesstoken);
    try 
         {
            var teamTagsList  = await client.createTeamworkTagAsync(teamId, teamTag);
            res.status(201).send();
         }
        catch (ex) 
        {
            console.error(ex);
            res.status(500).send();
        }
}

const getAppDetails = async (req, res) => {
    try 
    {
        var appId = process.env.MicrosoftAppId;
        res.status(201).send(appId);
    }
    catch (ex) 
    {
        console.error(ex);
        res.status(500).send();
    }
}

const deleteTagAsync = async (req, res) => {
    var teamId = req.query.teamId;
    var tagId = req.query.tagId;
    var tid = process.env.MicrosoftAppTenantId;
    var token = req.query.ssoToken;
    var accesstoken = await getDelegateAccessToken(tid, token);

    const client = new SimpleGraphClient(accesstoken);
    try 
         {
            var teamTagsList  = await client.deleteTeamworkTagsAsync(teamId, tagId);
            res.status(204).send();
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
    var tid = process.env.MicrosoftAppTenantId;
    var token = req.query.ssoToken;
    var accesstoken = await getDelegateAccessToken(tid, token);
   
    const client = new SimpleGraphClient(accesstoken);
        try 
         {
            var teamTagsList  = await client.listTeamworkTagsAsync(teamId);
            res.status(200).send(teamTagsList.value);
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
    var token = req.query.ssoToken;
    var tid = process.env.MicrosoftAppTenantId;
    var teamTag = req.body;
    var accesstoken = await getDelegateAccessToken(tid, token);

    const client = new SimpleGraphClient(accesstoken);
        try 
         {
            var teamTagsList  = await client.updateTeamworkTagAsync(teamId, teamTag);
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
    var tid = process.env.MicrosoftAppTenantId;
    var teamTagId = req.query.tagId;
    var token = req.query.ssoToken;
    var accesstoken = await getDelegateAccessToken(tid, token);
 
    const client = new SimpleGraphClient(accesstoken);
    try 
         {
            var tagsMemberList  = await client.getTeamworkTagsMembersAsync(teamId, teamTagId);
            res.status(200).send(tagsMemberList.value);
         }
        catch (ex) 
        {
            console.error(ex);
            res.status(500).send();
        }

}

// Exchange the id token with access token
const getDelegateAccessToken = async (tid, token) => {
    
    try {
    var result = await msalClient.acquireTokenOnBehalfOf({
        authority: `https://login.microsoftonline.com/${tid}`,
        oboAssertion: token,
        scopes: scopes,
        skipCache: true
      });
    }catch(error){
        console.log("Error occured"+error);
    }

    return result.accessToken;
}

module.exports = {
    createTeamTag,
    listTeamTagAsync,
    updateTeamTagAsync,
    getAppDetails,
    deleteTagAsync,
    getTeamworkTagMembersAsync
};