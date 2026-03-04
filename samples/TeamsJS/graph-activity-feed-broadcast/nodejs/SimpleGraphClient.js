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
    async getInstalledAppsForUser(userId) {
        return await this.graphClient
            .api('/users/'+ userId +'/teamwork/installedApps').expand('teamsAppDefinition').version('beta')
            .get().then((res) => {
                return res;
        });
    }

    async getUserList(){
        return await this.graphClient.api('/users').get();
    }

    async installAppForUser(userId,appId){
        const userScopeTeamsAppInstallation = {
            "teamsApp@odata.bind": "https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/" + appId
         };

        return await this.graphClient.api('/users/'+userId+'/teamwork/installedApps')
	                .post(userScopeTeamsAppInstallation);
    }
}

exports.SimpleGraphClient = SimpleGraphClient;