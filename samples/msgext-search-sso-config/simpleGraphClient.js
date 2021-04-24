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
            },
        });
    }

    /**
     *
     * @param {string} searchQuery text to search the inbox for.
     */
    async searchMailInbox(searchQuery) {
        // Searches the user's mail Inbox using the Microsoft Graph API
        return await this.graphClient
            .api(`me/messages?$search=\"${searchQuery}\"`)
            .get();
    }
    async GetMyProfile() {
        return await this.graphClient.api('/me').get();
    }

    // // Gets the user's photo
    // async GetPhotoAsync() {
    //     // let graphRequestParams = {
    //     //     method: 'GET',
    //     //     headers: {
    //     //         'Content-Type': 'image/png',
    //     //         "authorization": "bearer " + token
    //     //     }            
    //     // }
    //     const header = {
    //         'Content-Type': 'image/png'
    //     }
    //     let result = await this.graphClient.api('/me/photos/240x240/$value').headers(header).get();
    //     //let graphPhotoEndpoint = 'https://graph.microsoft.com/v1.0/me/photos/240x240/$value';


    //     //let response = await fetch(graphPhotoEndpoint, graphRequestParams).catch(this.unhandledFetchError);
    //     if (!result) {
    //         console.error("ERROR getting user photo!");
    //     }

    //     //let imageBuffer = await result.arrayBuffer().catch(this.unhandledFetchError); //Get image data as raw binary data
    //     //const imageBuffer = result.
    //     //Convert binary data to an image URL and set the url in state
    //     const imageUri = 'data:image/png;base64,' + Buffer.from(result).toString('base64');
    //     return imageUri;
    // }
    
    // Gets the user's photo
    async GetPhotoAsync(token) {
        let graphPhotoEndpoint = 'https://graph.microsoft.com/v1.0/me/photos/240x240/$value';
        let graphRequestParams = {
            method: 'GET',
            headers: {
                'Content-Type': 'image/png',
                "authorization": "bearer " + token
            }            
        }

        let response = await fetch(graphPhotoEndpoint, graphRequestParams).catch(this.unhandledFetchError);
        if (!response.ok) {
            console.error("ERROR: ", response);
        }

        let imageBuffer = await response.arrayBuffer().catch(this.unhandledFetchError); //Get image data as raw binary data

        //Convert binary data to an image URL and set the url in state
        const imageUri = 'data:image/png;base64,' + Buffer.from(imageBuffer).toString('base64');
        return imageUri;
    }
}

exports.SimpleGraphClient = SimpleGraphClient;
