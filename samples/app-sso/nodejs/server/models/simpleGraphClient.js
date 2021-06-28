// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { Client } = require('@microsoft/microsoft-graph-client');
require('isomorphic-fetch');

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
     *
     * @param {string} searchQuery text to search the inbox for.
     */
    async searchMailInbox(searchQuery) {
        // Searches the user's mail Inbox using the Microsoft Graph API
        return await this.graphClient
            .api('me/mailfolders/inbox/messages')
            .search(searchQuery)
            .get();
    }

    async getMyProfile() {
        try {
            return await this.graphClient.api('/me').get();
        } catch (error) {
            return {};
        }
    }

    // Gets the user's photo
    async getPhotoAsync() {
        const graphPhotoEndpoint = 'https://graph.microsoft.com/v1.0/me/photo/$value';
        const graphRequestParams = {
            method: 'GET',
            headers: {
                'Content-Type': 'image/png',
                authorization: 'bearer ' + this._token
            }
        };

        // eslint-disable-next-line no-undef
        const response = await fetch(graphPhotoEndpoint, graphRequestParams).catch(this.unhandledFetchError);
        if (!response || !response.ok) {
            console.error('User Image Not Found!!');
            // Return a Sample Image
            return 'https://adaptivecards.io/content/cats/1.png';
        }
        const imageBuffer = await response.arrayBuffer().catch(this.unhandledFetchError); // Get image data as raw binary data
        // Convert binary data to an image URL and set the url in state
        const imageUri = 'data:image/png;base64,' + Buffer.from(imageBuffer).toString('base64');
        return imageUri;
    }
}

exports.SimpleGraphClient = SimpleGraphClient;
