// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const auth = require("../auth");
const axios = require('axios');

class GraphHelper {
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
    * PageId =1 for Channel Subscription
    * PageId =2 for Team Subscription.
    * @param {pageId}. Team and channel subscription.
    */
    static async createSubscription(teamsId, pageId) {
        let applicationToken = await auth.getAccessToken();
        let resource = "";
        let changeType = "";

        if (pageId === "1") {
            resource = `/teams/${teamsId}/channels`;
            changeType = "created,deleted,updated";
        }
        else {
            resource = `/teams/${teamsId}`;
            changeType = "deleted,updated"
        }

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
        }
        catch (ex) {
            return null;
        }

        var existingSubscription = existingSubscriptions.find(subscription => subscription.Resource === resource);

        if (existingSubscription != null && existingSubscription.NotificationUrl != notificationUrl) {
            console.log(`CreateNewSubscription-ExistingSubscriptionFound: ${resource}`);
            await this.deleteSubscription(existingSubscription.id);
            existingSubscription = null;
        }

        try {

            if (existingSubscription == null) {

                let subscriptionCreationInformation = {
                    changeType: changeType,
                    notificationUrl: process.env.notificationUrl,
                    resource: resource,
                    includeResourceData: true,
                    encryptionCertificate: process.env.Base64EncodedCertificate,
                    encryptionCertificateId: "meeting-notification",
                    expirationDateTime: new Date(Date.now() + 3600000).toISOString(),
                    clientState: "clientState"
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