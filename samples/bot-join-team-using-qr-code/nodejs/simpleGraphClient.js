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


    // Get list of all teams in the organization
    async getAllTeams() {
        return await this.graphClient
            .api('/groups?$filter=resourceProvisioningOptions/Any(x:x eq \'Team\')').version('beta')
            .get().then((res) => {
                return res;
            });
    }

    // Add the user to the team whose team id is specified
    async joinTeam(userId, teamId) {
        const teamObject = {
            "@odata.type": "#microsoft.graph.aadUserConversationMember",
            "roles": [
                "owner"
            ],
            "user@odata.bind": "https://graph.microsoft.com/v1.0/users('" + userId + "')"
        };

        return await this.graphClient
            .api('teams/' + teamId + '/members').version('beta')
            .post(teamObject).then((res) => {
                return res;
            });
    }
}

exports.SimpleGraphClient = SimpleGraphClient;