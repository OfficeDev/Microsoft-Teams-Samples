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
    * Gets list of all pinned messages in chat.
    */
    async getPinnedMessageList(chatId) {
        return await this.graphClient
            .api('/chats/'+ chatId +'/pinnedMessages').version('beta')
            .expand('message')
            .get().then((res) => {
                return res;
        });
    }

    /**
    * Returns top 5 recent chat messages from chat.
    */
    async getRecentMessageList(chatId) {
        return await this.graphClient
            .api('/chats/'+ chatId +'/messages').version('beta')
            .orderby('createdDateTime desc')
            .top(5)
            .get().then((res) => {
                return res;
        });
    }

    /**
    * Pin new message in chat based on chatId passed.
    */
    async pinNewMessage(chatId, newMessageId) {

        const pinnedChatMessageInfo = {
            'message@odata.bind':'https://graph.microsoft.com/beta/chats/'+ chatId +'/messages/'+ newMessageId
         }

        return await this.graphClient
            .api('/chats/'+ chatId +'/pinnedMessages').version('beta')
            .post(pinnedChatMessageInfo)
            .then((res) => {
                return res;
        });
    }

    /**
    * Unpin the message from chat based on chatId passed.
    */
    async unpinMessage(chatId, pinnedMessageId) {
        return await this.graphClient
            .api('/chats/'+ chatId +'/pinnedMessages/'+pinnedMessageId)
            .version('beta')
            .delete().then((res) => {
                return res;
        });
    }
}

exports.SimpleGraphClient = SimpleGraphClient;