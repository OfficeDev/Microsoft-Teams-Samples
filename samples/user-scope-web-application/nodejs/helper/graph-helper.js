// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const auth = require("../auth");
const axios = require('axios');
var applicationToken = "";

class GraphHelper {

    /**
     * Get All Group Chats
     */
    static async getAllChatsFromGraphpAPI(userId, token) {
        let groupChatList = [];

        try {
            var apiResponse = await axios.get(`https://graph.microsoft.com/beta/users/${userId}/chats?notifyOnUserSpecificProperties=${true}`, {
                headers: {
                    "accept": "application/json",
                    "contentType": 'application/json',
                    "authorization": "bearer " + token
                }
            });

            groupChatList = [];
            (apiResponse.data.value).forEach(element => {
                if (element.viewpoint.isHidden === false && element.topic !== null) {
                    groupChatList.push(element);
                }
            });

            return groupChatList;
        }
        catch (ex) {
            return null;
        }
    }

    /**
     * Get All Messages by chatId
     */
    static async getAllMessagesByChatId(chatId, token) {
        let messagesList = [];

        try {
            var apiResponse = await axios.get(`https://graph.microsoft.com/v1.0/chats/${chatId}/messages`, {
                headers: {
                    "accept": "application/json",
                    "contentType": 'application/json',
                    "authorization": "bearer " + token
                }
            });

            messagesList = [];
            messagesList = apiResponse.data.value;

            if (messagesList.length > 0) {
                return messagesList;
            }
        }
        catch (ex) {
            return null;
        }
    }

    /**
     * Delete existing subscription.
     * @param {string} subscriptionId Id of subscription to be deleted.
     */
    static async deleteSubscription(subscriptionId) {
        await axios.delete(`https://graph.microsoft.com/v1.0/subscriptions/${subscriptionId}`, {
            headers: {
                "accept": "application/json",
                "contentType": 'application/json',
                "authorization": "bearer " + applicationToken
            }
        });
    }

    /**
    * id - It can be teams-id (or) chat-id/user-id/teams-app-id
    * subsId - Is a types of subscription for different resources of graph API.
    * @param {pageId}. Team and channel subscription.
    */
    static async createSubscription(id, token, subsId) {
        let existing_Subscriptions = null;
        applicationToken = token;
        let resource = "";
        let changeType = "";

        switch (subsId) {
            case 'channel':
                resource = `/teams/${id}/channels`; // id = teamsId
                changeType = "created,deleted,updated";
                break;
            case 'team':
                resource = `/teams/${id}`; // id = teamsId
                changeType = "deleted,updated"
                break;
            case 'specificChat':
                resource = `/chats/${id}`; // id = group-chat id
                changeType = "updated";
                break;
            case 'anyChat':
                resource = `/chats`;
                changeType = "created,updated";
                break;
            case 'userSpecificWithChatID':
                resource = `/chats/${id}?notifyOnUserSpecificProperties=${true}`; // id = group-chat id
                changeType = "created,updated";
                break;
            case 'userLevel':
                let userId = id;
                resource = `/users/${userId}/chats`; //id = userId
                changeType = "created,updated";
                break;
            case 'mePath':
                resource = `/me/chats`;
                changeType = "created,updated";
                break;
            case 'userLeveOnUserSpecificProperty':
                resource = `/users/${id}/chats?notifyOnUserSpecificProperties=true`; // id = userId
                changeType = "created,updated";
                break;
            case 'whereTeamsAppInstalled':
                let teamsAppId = id;
                resource = `/appCatalogs/teamsApps/${teamsAppId}/installedToChats`; // id = teams-app-id
                changeType = "created,updated";
                break;
        }

        // Check For Existing Subscription
        try {
            var apiResponse = await axios.get(`https://graph.microsoft.com/beta/subscriptions`, {
                headers: {
                    "accept": "application/json",
                    "contentType": 'application/json',
                    "authorization": "bearer " + applicationToken
                }
            });

            existing_Subscriptions = apiResponse.data.value;

            var existingSubscription = existing_Subscriptions.find(subscription => subscription.resource === resource);

            // Delete old subscriotion, If subscription exists
            if (existingSubscription) {
                if (existingSubscription.resource != resource) {
                    await this.deleteSubscription(existingSubscription.id);
                    existingSubscription = null;
                    console.log("Previous Subscription Deleted Successfully");
                }
                else {
                    console.log(`ExistingSubscriptionFound: ${resource}`);
                    return "Using Existing Subscription"
                }
            }
        }
        catch (ex) {
            return null;
        }

        try {
            if (existingSubscription == "" || existingSubscription == null || existingSubscription == undefined) {
                let subscriptionCreationInformation = {
                    changeType: changeType,
                    notificationUrl: process.env.notificationUrl,
                    resource: resource,
                    includeResourceData: true,
                    encryptionCertificate: process.env.Base64EncodedCertificate,
                    encryptionCertificateId: "graphNotifications",
                    expirationDateTime: new Date(Date.now() + 3600000).toISOString(),
                    clientState: "secretClientState"
                };

                var response = await axios.post(`https://graph.microsoft.com/beta/subscriptions`, subscriptionCreationInformation, {
                    headers: {
                        "accept": "application/json",
                        "contentType": 'application/json',
                        "authorization": "bearer " + applicationToken
                    }
                });

                existingSubscription = response.data;
                return "Subscription Created";
            }
        }
        catch (e) {
            console.log("Error--" + e);
        }
    }
}

exports.GraphHelper = GraphHelper;