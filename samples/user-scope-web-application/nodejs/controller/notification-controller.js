// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { GraphHelper } = require("../helper/graph-helper");

/** Check Existing Subscriotion */
const checkExistingSubsription = async (req, res) => {
    try {
        await GraphHelper.checkExistingSubscription();
        let existingResource = await GraphHelper.checkExistingSubscription();
        console.log(req);

        if (existingResource != null) {
            console.log("EXISTING SUBSCRIPTION IS : ", existingResource);
            res.status(202).send(existingResource);
        }
        else {
            console.log("NO EXISTING SUBSCRIPTION FOUND");
            res.status(202).send();
        }
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

/** Creates Subscription for Channel */
const createChannelAsync = async (req, res) => {
    var teamId = req.query.teamId;
    var subsId = "channel";

    try {
        await GraphHelper.createSubscription(teamId, subsId);
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
    var subsId = "team";

    try {
        var result = await GraphHelper.createSubscription(teamId, subsId);
        res.status(202).send(result);
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

/** Creates Subscription for UserSCope Chats */
const subscribeToSpecificChat = async (req, res) => {
    var chatId = req.query.chatId;
    var subsId = "specificChat";

    try {
        var result = await GraphHelper.createSubscription(chatId, subsId);
        res.status(202).send(result);
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

/** Creates Subscription for UserSCope Chats */
const subscribeToAnyChat = async (req, res) => {
    var chatId = req.query.chatId;
    var subsId = "anyChat";

    try {
        var result = await GraphHelper.createSubscription(chatId, subsId);
        res.status(202).send(result);
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

/** Creates Subscription for UserSpecificProperties */
const notifyOnUserSpecificProperties = async (req, res) => {
    var chatId = req.query.chatId;
    var subsId = "userSpecificWithChatID";

    try {
        var result = await GraphHelper.createSubscription(chatId, subsId);
        res.status(202).send(result);
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

/** Creates Subscription for userLevelChats */
const userLevelChats = async (req, res) => {
    var userId = req.query.userId;
    var subsId = "userLevel";

    try {
        var result = await GraphHelper.createSubscription(userId, subsId);
        res.status(202).send(result);
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

/** Creates Subscription for userLevelUsingMePath */
const userLevelUsingMePath = async (req, res) => {
    var userId = req.query.userId;
    var subsId = "mePath";

    try {
        var result = await GraphHelper.createSubscription(userId, subsId);
        res.status(202).send(result);
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

/** Creates Subscription for userLevelChatsUsingNotifyOnUserSpecificProperties */
const userLevelChatsUsingNotifyOnUserSpecificProperties = async (req, res) => {
    var userId = req.query.userId;
    var token = req.query.token;
    var subId = "userLeveOnUserSpecificProperty";

    try {
        var result = await GraphHelper.createSubscription(userId, token, subId);
        res.status(202).send(result);
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

/** Creates Subscription for anyChatWhereTeamsAppIsInstalled */
const anyChatWhereTeamsAppIsInstalled = async (req, res) => {
    var userId = req.query.userId;
    var subsId = "whereTeamsAppInstalled";

    try {
        var result = await GraphHelper.createSubscription(userId, subsId);
        res.status(202).send(result);
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

/** Get all chats */
const getAllChats = async (req, res) => {
    var userId = req.query.userId;

    try {
        var result = await GraphHelper.getAllChatsFromGraphpAPI(userId);
        res.status(202).send(result);
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

/** Get all messages base on chat-id */
const getAllMessageByChatId = async (req, res) => {
    var chatId = req.query.chatId;

    try {
        var result = await GraphHelper.getAllMessagesByChatId(chatId);
        res.status(202).send(result);
    }
    catch (ex) {
        console.error(ex);
        res.status(500).send();
    }
}

module.exports = {
    createChannelAsync,
    createTeamAsync,
    subscribeToSpecificChat,
    subscribeToAnyChat,
    notifyOnUserSpecificProperties,
    userLevelChats,
    userLevelUsingMePath,
    userLevelChatsUsingNotifyOnUserSpecificProperties,
    anyChatWhereTeamsAppIsInstalled,
    checkExistingSubsription,
    getAllChats,
    getAllMessageByChatId
};