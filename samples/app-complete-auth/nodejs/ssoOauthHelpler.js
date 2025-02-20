const { StatusCodes, ActivityTypes, tokenExchangeOperationName } = require('botbuilder');

/**
 * SsoOAuthHelpler class handles the SSO OAuth helper functions.
 */
class SsoOAuthHelpler {
    /**
     * Creates a new instance of the SsoOAuthHelpler class.
     * @param {string} oAuthConnectName - The OAuth connection name.
     * @param {Object} storage - The storage object.
     */
    constructor(oAuthConnectName, storage) {
        this.oAuthConnectName = oAuthConnectName;
        this.storage = storage;
    }

    /**
     * Determines if a "signin/tokenExchange" should be processed by this caller.
     * If a token exchange is unsuccessful, an InvokeResponse of PreconditionFailed is sent.
     * @param {Object} turnContext - The context for this specific activity.
     * @returns {Promise<boolean>} - True if the bot should continue processing this TokenExchange request.
     */
    async shouldProcessTokenExchange(turnContext) {
        if (turnContext.activity.name !== tokenExchangeOperationName) {
            throw new Error("Only 'signin/tokenExchange' invoke activities can be processed by TokenExchangeHelper.");
        }

        if (!await this.exchangedToken(turnContext)) {
            // If the TokenExchange is NOT successful, the response will have already been sent by ExchangedToken
            return false;
        }

        // If a user is signed into multiple Teams clients, the Bot might receive a "signin/tokenExchange" from each client.
        // Each token exchange request for a specific user login will have an identical activity.value.Id.
        // Only one of these token exchange requests should be processed by the bot. For a distributed bot in production,
        // this requires a distributed storage to ensure only one token exchange is processed.

        // This example utilizes Bot Framework IStorage's ETag implementation for token exchange activity deduplication.

        // Create a StoreItem with Etag of the unique 'signin/tokenExchange' request
        const storeItem = {
            eTag: turnContext.activity.value.id
        };

        return true;
    }

    /**
     * Exchanges the token.
     * @param {Object} turnContext - The context for this specific activity.
     * @returns {Promise<boolean>} - True if the token exchange was successful.
     */
    async exchangedToken(turnContext) {
        let tokenExchangeResponse = null;
        const tokenExchangeRequest = turnContext.activity.value;

        try {
            // turnContext.adapter IExtendedUserTokenProvider
            tokenExchangeResponse = await turnContext.adapter.exchangeToken(
                turnContext,
                tokenExchangeRequest.connectionName,
                turnContext.activity.from.id,
                { token: tokenExchangeRequest.token });
                
            console.log('tokenExchangeResponse: ' + JSON.stringify(tokenExchangeResponse));
        } catch (err) {
            console.log(err);
            // Ignore Exceptions
            // If token exchange failed for any reason, tokenExchangeResponse above stays null, and hence we send back a failure invoke response to the caller.
        }

        if (!tokenExchangeResponse || !tokenExchangeResponse.token) {
            // The token could not be exchanged (which could be due to a consent requirement)
            // Notify the sender that PreconditionFailed so they can respond accordingly.
            await turnContext.sendActivity({
                type: ActivityTypes.InvokeResponse,
                value: {
                    status: StatusCodes.PRECONDITION_FAILED,
                    // TokenExchangeInvokeResponse
                    body: {
                        id: tokenExchangeRequest.id,
                        connectionName: tokenExchangeRequest.connectionName,
                        failureDetail: 'The bot is unable to exchange token. Proceed with regular login.'
                    }
                }
            });

            return false;
        } else {
            // Store response in TurnState, so the SsoOAuthPrompt can use it, and not have to do the exchange again.
            turnContext.turnState.tokenExchangeInvokeRequest = tokenExchangeRequest;
            turnContext.turnState.tokenResponse = tokenExchangeResponse;
        }

        return true;
    }

    /**
     * Gets the storage key.
     * @param {Object} turnContext - The context for this specific activity.
     * @returns {string} - The storage key.
     */
    getStorageKey(turnContext) {
        if (!turnContext || !turnContext.activity || !turnContext.activity.conversation) {
            throw new Error('Invalid context, cannot get storage key!');
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