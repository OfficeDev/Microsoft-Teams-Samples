// <copyright file="simpleGraphClient.js" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

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
    async getMeAsync() {
        return await this.graphClient.api('/me').get();
    }
}

exports.SimpleGraphClient = SimpleGraphClient;