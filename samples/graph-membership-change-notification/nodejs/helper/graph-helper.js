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
        debugger;
        let applicationToken = await auth.getAccessToken();
        let resource = "";
        let changeType = "";
        let notificationUrl = process.env.notificationUrl;

        if (pageId === "1") {
            resource = `/teams/${teamsId}/channels/getAllMembers?notifyOnIndirectMembershipUpdate=true&suppressNotificationWhenSharedUnsharedWithTeam=true`;
            changeType = "created,deleted,updated";
        }

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

        var existingSubscription = existingSubscriptions.find(subscription => subscription.resource === resource);

        if (existingSubscription != null && existingSubscription.notificationUrl != notificationUrl) {
            console.log(`CreateNewSubscription-ExistingSubscriptionFound: ${resource}`);
            await this.deleteSubscription(existingSubscription.id);
            existingSubscription = null;
        }

        try {            
            
            if (existingSubscription == null) {

                let subscriptionCreationInformation = {
                    changeType: changeType,
                    notificationUrl: notificationUrl,
                    lifecycleNotificationUrl: notificationUrl, 
                    resource: resource,
                    includeResourceData: true,
                    encryptionCertificate: process.env.Base64EncodedCertificate,
                    encryptionCertificateId: "meeting-notification",
                    expirationDateTime: new Date(Date.now() + 36000000).toISOString(),
                    clientState: "clientState"
                };

                var response = await axios.post(`https://graph.microsoft.com/beta/subscriptions`,subscriptionCreationInformation, {
                    headers: {
                        "accept": "application/json",
                        "contentType": 'application/json',
                        "authorization": "bearer " + applicationToken
                    }
                });

                existingSubscription = response.data;
            }
        }
        catch (e) {
            console.log("Error--" + e);
        }

        return existingSubscription;
    }

    /**
    * PageId =1 for Channel Subscription
    * PageId =2 for Team Subscription.
    * @param {pageId}. Team and channel subscription.
    */
    static async createSharedWithTeamSubscription(teamsId, channelId, pageId) {
        debugger;
        
        // Parameter validation
        if (!teamsId || !channelId) {
            console.error("teamsId and channelId are required parameters");
            return null;
        }
        
        // Default pageId to "1" if undefined
        if (!pageId) {
            console.log("pageId is undefined, defaulting to '1' for Channel Subscription");
            pageId = "1";
        }
        
        let applicationToken = await auth.getAccessToken();
        let resource = "";
        let changeType = "";
        let notificationUrl = process.env.notificationUrl;

        if (pageId === "1") {
            resource = `/teams/${teamsId}/channels/${channelId}/sharedWithTeams`;
            changeType = "created,deleted";
        }

        let existingSharedWithTeamSubscriptions = null;

        try {
            var apiResponse = await axios.get(`https://graph.microsoft.com/beta/subscriptions`, {
                headers: {
                    "accept": "application/json",
                    "contentType": 'application/json',
                    "authorization": "bearer " + applicationToken
                }
            });

            existingSharedWithTeamSubscriptions = apiResponse.data.value;
        }
        catch (ex) {
            return null;
        }

        var existingSharedWithTeamSubscription = existingSharedWithTeamSubscriptions.find(subscription => subscription.resource === resource);

        if (existingSharedWithTeamSubscription != null && existingSharedWithTeamSubscription.notificationUrl != notificationUrl) {
            console.log(`CreateNewSubscription-ExistingSubscriptionFound: ${resource}`);
            await this.deleteSubscription(existingSharedWithTeamSubscription.id);
            existingSharedWithTeamSubscription = null;
        }

        try {

            if (existingSharedWithTeamSubscription == null) {

                let subscriptionCreationInformation = {
                    changeType: changeType,
                    notificationUrl: notificationUrl,
                    lifecycleNotificationUrl: notificationUrl, 
                    resource: resource,
                    includeResourceData: true,
                    encryptionCertificate: process.env.Base64EncodedCertificate,
                    encryptionCertificateId: "meeting-notification",
                    expirationDateTime: new Date(Date.now() + 36000000).toISOString(),
                    clientState: "clientState"
                };

                var response = await axios.post(`https://graph.microsoft.com/beta/subscriptions`,subscriptionCreationInformation, {
                    headers: {
                        "accept": "application/json",
                        "contentType": 'application/json',
                        "authorization": "bearer " + applicationToken
                    }
                });

                existingSharedWithTeamSubscription = response.data;
            }
        }
        catch (e) {
            console.log("Error--" + e);
        }

        return existingSharedWithTeamSubscription;
    }
    
    /**
     * Check if a user has access to a specific channel
     * @param {string} teamId Team ID
     * @param {string} channelId Channel ID
     * @param {string} userId User ID
     * @param {string} tenantId Tenant ID
     * @returns {Promise<boolean>} True if the user has access, false otherwise
     */
    static async checkUserChannelAccess(teamId, channelId, userId, tenantId) {
        try {
            const applicationToken = await auth.getAccessToken();
            const response = await axios.get(
                `https://graph.microsoft.com/v1.0/teams/${teamId}/channels/${channelId}/microsoft.graph.doesUserHaveAccess(userId='${userId}', tenantId='${tenantId}')`,
                {
                    headers: {
                        "accept": "application/json",
                        "authorization": "bearer " + applicationToken
                    }
                }
            );
            return response.data.value;
        } catch (error) {
            console.log("Error checking user channel access:", error);
            return false;
        }
    }
}

exports.GraphHelper = GraphHelper;