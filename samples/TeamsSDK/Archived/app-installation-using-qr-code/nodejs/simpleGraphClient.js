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
    * Install app in a team
    */
    async installApp(appId,teamId) {
        const teamsAppInstallation = {
            'teamsApp@odata.bind':'https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/'+ appId
         };
         
         await this.graphClient.api('/teams/'+ teamId +'/installedApps')
             .post(teamsAppInstallation);
    }
}

exports.SimpleGraphClient = SimpleGraphClient;