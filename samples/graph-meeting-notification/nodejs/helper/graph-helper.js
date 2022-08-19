
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const auth = require("../auth");
const axios = require('axios');;

class GraphHelper {

    /**
     * Delete existing subscription.
     * @param {string} subscriptionId Id of subscription to be deleted.
     */
    static async deleteSubscription(subscriptionId) {
        var applicationToken = await auth.getAccessToken();

        await axios.delete(`https://graph.microsoft.com/beta/subscriptions/${subscriptionId}`, {
            headers: {
                "accept": "application/json",
                "contentType": 'application/json',
                "authorization": "bearer " + applicationToken
            }
        });
    }

    static async createSubscription(meetingJoinUrl) {
        let applicationToken = await auth.getAccessToken();
        let resource = `/communications/onlineMeetings/?$filter=JoinWebUrl eq '${meetingJoinUrl}'`;

        let existingSubscriptions = null;

        try {

            var apiResponse = await axios.get(`https://graph.microsoft.com/beta/subscriptions`, {
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
                    resource: resource,
                    notificationUrl: process.env.notificationUrl,
                    expirationDateTime: new Date(Date.now() + 3600000).toISOString(),
                    includeResourceData: true,
                    changeType: "updated",
                    clientState: "ClientState",
                    encryptionCertificate: process.env.Base64EncodedCertificate,
                    encryptionCertificateId: process.env.EncryptionCertificateId
                };



                var response = await axios.post(`https://graph.microsoft.com/beta/subscriptions`, subscriptionCreationInformation, {
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