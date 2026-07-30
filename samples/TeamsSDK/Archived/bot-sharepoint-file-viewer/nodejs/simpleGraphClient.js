// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { Client } = require('@microsoft/microsoft-graph-client');

/**
* This class is a wrapper for the Microsoft Graph API.
* See: https://developer.microsoft.com/en-us/graph for more information.
*/
class SimpleGraphClient {
    constructor(token) {
        if (!token || !token.trim()) {
            throw new Error('SimpleGraphClient: Invalid token received.');
        }

        this._token = token;

        // Get an Authenticated Microsoft Graph client using the token issued to the user.
        this.graphClient = Client.init({
            authProvider: (done) => {
                done(null, this._token); // First parameter takes an error if you can't get an access token.
            }
        });
    }

    /**
    * Collects information about the user in the bot.
    */
    async getSiteDetails(sharepointTenantId, sharepointSiteName) {
        return await this.graphClient
            .api('/sites/'+sharepointTenantId+':/sites/'+sharepointSiteName+'').version('beta')
            .get().then((res) => {
                return res;
            });
    }

    /**
    * Collects information about the SharePoint drive based on SharePoint id passed.
    */
    async getDriveDetails(siteId) {
        return await this.graphClient
            .api('/sites/'+siteId+'/drives').version('beta')
            .get().then((res) => {
                return res;
            });
    }

    /**
    * Collects information about the documents stored in SharePoint drive.
    */
    async getContentList(siteId, driveId) {
        return await this.graphClient
            .api('/sites/'+siteId+'/drives/'+driveId+'/root/children').version('beta')
            .get().then((res) => {
                return res;
            });
    }

    /**
    * This endpoint will upload the user's file to SharePoint drive.
    */
    async uploadFile(siteId, driveId, stream, fileName) {
        return await this.graphClient
            .api('/sites/'+siteId+'/drives/'+driveId+'/root:/'+fileName+':/content').version('beta')
            .put(stream).then((res) => {
                return res;
            });
    }
}

exports.SimpleGraphClient = SimpleGraphClient;