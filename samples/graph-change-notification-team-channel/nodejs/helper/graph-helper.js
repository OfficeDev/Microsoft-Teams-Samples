// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const auth = require("../auth");
const axios = require('axios');

class GraphHelper {

    /**
     * Check existing subscription for to show active
     */
    static async checkExistingSubscription() {
        let applicationToken = await auth.getAccessToken();
        let existingSubscriptions = null;

        try {
            var apiResponse = await axios.get(`https://graph.microsoft.com/v1.0/subscriptions`, {
                headers: {
                    "accept": "application/json",
                    "contentType": 'application/json',
                    "authorization": "bearer " + applicationToken
                }
            });

            existingSubscriptions = apiResponse.data.value;

            let ExistingResource = "";

            if (existingSubscriptions.length > 0) {
                ExistingResource = existingSubscriptions[0].resource;
                return ExistingResource;
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
        var applicationToken = await auth.getAccessToken();

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
    static async createSubscription(id, subsId) {
        let applicationToken = await auth.getAccessToken();
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
                resource = `/users/${id}/chats?notifyOnUserSpecificProperties=${true}`; // id = userId
                changeType = "created,updated";
                break;
            case '9':
                let teamsAppId = id;
                resource = `/appCatalogs/teamsApps/${teamsAppId}/installedToChats`; // id = teams-app-id
                changeType = "created,updated";
                break;
        }

        let existingSubscription = null;

        try {
            var apiResponse = await axios.get(`https://graph.microsoft.com/v1.0/subscriptions`, {
                headers: {
                    "accept": "application/json",
                    "contentType": 'application/json',
                    "authorization": "bearer " + applicationToken
                }
            });

            existingSubscription = apiResponse.data.value;

            // Delete old subscriotion, If subscription exists
            if (existingSubscription.length > 0) {
                if (existingSubscription[0].resource != resource) {
                    console.log(`CreateNewSubscription-ExistingSubscriptionFound: ${resource}`);
                    await this.deleteSubscription(existingSubscription[0].id);
                    existingSubscription = null;
                    console.log("Previous Subscription Deleted Successfully");
                }
            }
        }
        catch (ex) {
            return null;
        }

        try {
            if (existingSubscription == "" || existingSubscription == null) {
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

                var response = await axios.post(`https://graph.microsoft.com/v1.0/subscriptions`, subscriptionCreationInformation, {
                    headers: {
                        "accept": "application/json",
                        "contentType": 'application/json',
                        "authorization": "bearer " + applicationToken
                    }
                });

                existingSubscription = response.data.value;
            }
        }
        catch (e) {
             console.log("Error--" + e);
        }

        return existingSubscription;
    }
}

exports.GraphHelper = GraphHelper;