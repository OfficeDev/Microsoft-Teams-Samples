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
        try {
            return await this.graphClient
                .api('/sites/'+sharepointTenantId+'.sharepoint.com,/sites/'+sharepointSiteName+'')
                .get().then((res) => {
                    return res;
                });
        } catch (error) {
            try {
                return await this.graphClient
                    .api('/sites/'+sharepointTenantId+'.sharepoint.com:/sites/'+sharepointSiteName+':')
                    .get().then((res) => {
                        return res;
                    });
            } catch (altError) {
                return null;
            }
        }
    }

    /**
    * Collects information about the SharePoint drive based on SharePoint id passed.
    */
    async getDriveDetails(siteId) {
        try {
            const result = await this.graphClient
                .api('/sites/'+siteId+'/drives').version('beta')
                .get().then((res) => {
                    return res;
                });
            
            // If no drives found, try to get the default drive
            if (!result || !result.value || result.value.length === 0) {
                try {
                    const defaultDrive = await this.graphClient
                        .api('/sites/'+siteId+'/drive').version('beta')
                        .get();
                    
                    // Format it to match the expected structure
                    return {
                        value: [defaultDrive]
                    };
                } catch (defaultError) {
                    return null;
                }
            }
            
            return result;
        } catch (error) {
            
            // Try alternative approach - get the default drive
            try {
                const defaultDrive = await this.graphClient
                    .api('/sites/'+siteId+'/drive').version('beta')
                    .get();
                
                // Format it to match the expected structure
                return {
                    value: [defaultDrive]
                };
            } catch (defaultError) {
                return null;
            }
        }
    }

    /**
    * Collects information about the documents stored in SharePoint drive.
    */
    async getContentList(siteId, driveId) {
        try {
            return await this.graphClient
                .api('/sites/'+siteId+'/drives/'+driveId+'/root/children').version('beta')
                .get().then((res) => {
                    return res;
                });
        } catch (error) {
            return null;
        }
    }

    /**
    * This endpoint will upload the user's file to SharePoint drive.
    */
    async uploadFile(siteId, driveId, stream, fileName) {
        try {
            return await this.graphClient
                .api('/sites/'+siteId+'/drives/'+driveId+'/root:/'+fileName+':/content').version('beta')
                .put(stream).then((res) => {
                    return res;
                });
        } catch (error) {
            return null;
        }
    }
}

exports.SimpleGraphClient = SimpleGraphClient;