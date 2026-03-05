// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { Client } = require('@microsoft/microsoft-graph-client');
const fetch = require('node-fetch');

/**
 * This class is a wrapper for the Microsoft Graph API.
 * See: https://developer.microsoft.com/en-us/graph for more information.
 */
class SimpleGraphClient {
    /**
     * Creates an instance of SimpleGraphClient.
     * @param {string} token - The token issued to the user.
     */
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
     * @returns {Promise<Object>} The user information.
     */
    async getMe() {
        try {
            const res = await this.graphClient.api('/me').get();
            return res;
        } catch (error) {
            console.error('Error getting user information:', error);
            throw error;
        }
    }

    /**
     * Gets the user's photo.
     * @param {string} token - The token issued to the user.
     * @returns {Promise<string>} The user's photo as a base64 encoded string.
     */
    async getPhotoAsync(token) {
        const graphPhotoEndpoint = 'https://graph.microsoft.com/v1.0/me/photos/240x240/$value';
        const graphRequestParams = {
            method: 'GET',
            headers: {
                'Content-Type': 'image/png',
                'Authorization': `Bearer ${token}`
            }
        };

        try {
            const response = await fetch(graphPhotoEndpoint, graphRequestParams);
            if (!response.ok) {
                console.error('Error fetching photo:', response);
                throw new Error('Error fetching photo');
            }

            const imageBuffer = await response.arrayBuffer();
            const imageUri = `data:image/png;base64,${Buffer.from(imageBuffer).toString('base64')}`;
            return imageUri;
        } catch (error) {
            console.error('Error fetching photo:', error);
            throw error;
        }
    }
}

module.exports.SimpleGraphClient = SimpleGraphClient;