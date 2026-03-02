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
    * @param {pageId}. Team and channel subscription.
    */
    static async createSubscription(teamsId, pageId) {
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
            console.log("Error getting existing subscriptions:", ex);
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
                console.log(`Successfully created subscription for ${resource}`);
            }
        }
        catch (e) {
            console.log("Error creating subscription:", e.response?.data || e.message);
            
            // Check if error is due to app not being enabled in shared channel
            if (e.response?.data?.error?.code === 'Forbidden' || 
                e.response?.data?.error?.code === 'BadRequest') {
                console.log("Subscription creation failed - possibly due to app not being enabled in shared channel");
                throw new Error(`Subscription creation failed for ${resource}. The app may not be enabled in the shared channel.`);
            }
            
            throw new Error(`Failed to create subscription: ${e.response?.data?.error?.message || e.message}`);
        }

        return existingSubscription;
    }

    /**
    * PageId =1 for Channel Subscription
    * PageId =2 for Team Subscription.
    * @param {pageId}. Team and channel subscription.
    */
    static async createSharedWithTeamSubscription(teamsId, channelId, pageId) {
        // Parameter validation
        if (!teamsId || !channelId) {
            console.error("teamsId and channelId are required parameters");
            return null;
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
                console.log(`Successfully created shared channel subscription for ${resource}`);
            }
        }
        catch (e) {
            console.log("Error creating shared channel subscription:", e.response?.data || e.message);
            
            // Check if error is due to app not being enabled in shared channel
            if (e.response?.data?.error?.code === 'Forbidden' || 
                e.response?.data?.error?.code === 'BadRequest') {
                console.log("Shared channel subscription creation failed - app may not be enabled in shared channel");
                throw new Error(`Shared channel subscription creation failed for ${resource}. The app may not be enabled in the shared channel.`);
            }
            
            throw new Error(`Failed to create shared channel subscription: ${e.response?.data?.error?.message || e.message}`);
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

    /**
     * Get all members of a channel
     * @param {string} teamId Team ID
     * @param {string} channelId Channel ID
     * @returns {Promise<Array>} Array of channel members
     */
    static async getChannelMembers(teamId, channelId) {
        try {
            const applicationToken = await auth.getAccessToken();
            const response = await axios.get(
                `https://graph.microsoft.com/v1.0/teams/${teamId}/channels/${channelId}/allMembers`,
                {
                    headers: {
                        "accept": "application/json",
                        "authorization": "bearer " + applicationToken
                    }
                }
            );
            return response.data.value || [];
        } catch (error) {
            console.log("Error getting channel members:", error);
            return [];
        }
    }
}

exports.GraphHelper = GraphHelper;