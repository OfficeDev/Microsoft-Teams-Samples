// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { StatusCodes, ActivityTypes, tokenExchangeOperationName } = require('botbuilder');

class SsoOAuthHelpler {
    constructor(oAuthConnectName, storage) {
        this.oAuthConnectName = oAuthConnectName;
        this.storage = storage;
    }

    async shouldProcessTokenExchange(turnContext) {
        if (turnContext.activity.name !== tokenExchangeOperationName) {
            throw new Error("Only 'signin/tokenExchange' invoke activities can be processed by TokenExchangeHelper.");
        }

        if (!await this.exchangedToken(turnContext)) {
            return false;
        }

        return true;
    }

    async exchangedToken(turnContext) {
        let tokenExchangeResponse = null;
        const tokenExchangeRequest = turnContext.activity.value;

        try {
            tokenExchangeResponse = await turnContext.adapter.exchangeToken(
                turnContext,
                tokenExchangeRequest.connectionName,
                turnContext.activity.from.id,
                { token: tokenExchangeRequest.token });
                
            console.log('Token exchange successful');
        } catch (err) {
            console.error('Token exchange failed:', err.message);
        }

        if (!tokenExchangeResponse || !tokenExchangeResponse.token) {
            await turnContext.sendActivity(
                {
                    type: ActivityTypes.InvokeResponse,
                    value: {
                        status: StatusCodes.PRECONDITION_FAILED,
                        body: {
                            id: tokenExchangeRequest.id,
                            connectionName: tokenExchangeRequest.connectionName,
                            failureDetail: 'The bot is unable to exchange token. Proceed with regular login.'
                        }
                    }
                });

            return false;
        } else {
            turnContext.turnState.tokenExchangeInvokeRequest = tokenExchangeRequest;
            turnContext.turnState.tokenResponse = tokenExchangeResponse;
        }

        return true;
    }

    getStorageKey(turnContext) {
        if (!turnContext || !turnContext.activity || !turnContext.activity.conversation) {
            throw new Error('Invalid context, cannot get storage key.');
        }

        const activity = turnContext.activity;
        const channelId = activity.channelId;
        const conversationId = activity.conversation.id;

        if (activity.type !== ActivityTypes.Invoke || activity.name !== tokenExchangeOperationName) {
            throw new Error('TokenExchangeState can only be used with Invokes of signin/tokenExchange.');
        }

        const value = activity.value;
        if (!value || !value.id) {
            throw new Error('Invalid signin/tokenExchange. Missing activity.value.id.');
        }

        return `${channelId}/${conversationId}/${value.id}`;
    }
}

exports.SsoOAuthHelpler = SsoOAuthHelpler;