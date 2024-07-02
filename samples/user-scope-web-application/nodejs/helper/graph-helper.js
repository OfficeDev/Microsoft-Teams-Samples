// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const auth = require("../auth");
const axios = require('axios');
var applicationToken = "";

class GraphHelper {

    /**
     * Get All Group Chats
     */
    static async getAllChatsFromGraphpAPI(userId) {
        let applicationToken = await auth.getAccessToken();
        let groupChatList = [];

        try {
            var apiResponse = await axios.get(`https://graph.microsoft.com/v1.0/users/${userId}/chats?notifyOnUserSpecificProperties= ${true}`, {
                headers: {
                    "accept": "application/json",
                    "contentType": 'application/json',
                    "authorization": "bearer " + applicationToken
                }
            });

            groupChatList = [];
            groupChatList = apiResponse.data.value;
            if (groupChatList.length > 0) {
                return groupChatList;
            }
        }
        catch (ex) {
            return null;
        }
    }

    /**
     * Get All Messages by chatId
     */
    static async getAllMessagesByChatId(chatId) {
        let applicationToken = await auth.getAccessToken();
        let messagesList = [];

        try {
            var apiResponse = await axios.get(`https://graph.microsoft.com/v1.0/chats/${chatId}/messages`, {
                headers: {
                    "accept": "application/json",
                    "contentType": 'application/json',
                    "authorization": "bearer " + applicationToken
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
        console.log(new Date(Date.now() + 3600000).toISOString())
        let existing_Subscriptions = null;
        applicationToken = token;
        let resource = "";
        let changeType = "";

        switch (subsId) {
            case '1':
                resource = `/teams/${id}/channels`; // id = teamsId
                changeType = "created,deleted,updated";
                break;
            case '2':
                resource = `/teams/${id}`; // id = teamsId
                changeType = "deleted,updated"
                break;
            case '3':
                resource = `/chats/${id}`; // id = group-chat id
                changeType = "updated";
                break;
            case '4':
                resource = `/chats`;
                changeType = "created,updated";
                break;
            case '5':
                resource = `/chats/${id}?notifyOnUserSpecificProperties=${true}`; // id = group-chat id
                changeType = "created,updated";
                break;
            case '6':
                let userId = id;
                resource = `/users/${userId}/chats`; //id = userId
                changeType = "created,updated";
                break;
            case '7':
                resource = `/me/chats`;
                changeType = "created,updated";
                break;
            case '8':
                resource = `/users/${id}/chats?notifyOnUserSpecificProperties=true`; // id = userId
                changeType = "created,updated";
                break;
            case '9':
                let teamsAppId = id;
                resource = `/appCatalogs/teamsApps/${teamsAppId}/installedToChats`; // id = teams-app-id
                changeType = "created,updated";
                break;
            case '10':
                // Get All Messages For Particular User
                resource = `/users/${id}/chats`; // id = teams-app-id
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
                    console.log(`CreateNewSubscription-ExistingSubscriptionFound: ${resource}`);
                    return existingSubscription
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
                return existingSubscription;
            }
        }
        catch (e) {
            console.log("Error--" + e.response.data.error);
        }
    }
}

exports.GraphHelper = GraphHelper;