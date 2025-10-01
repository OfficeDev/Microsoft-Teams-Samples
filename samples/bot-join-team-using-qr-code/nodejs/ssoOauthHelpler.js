const { ActivityTypes, tokenExchangeOperationName } = require('botbuilder');
const { StatusCodes } = require('http-status-codes');

class SsoOAuthHelpler {
    constructor(oAuthConnectName, storage) {
        this.oAuthConnectName = oAuthConnectName;
        this.storage = storage;
    }

    /// <summary>
    /// Determines if a "signin/tokenExchange" should be processed by this caller.
    ///
    /// If a token exchange is unsuccessful, an InvokeResponse of PreconditionFailed is sent.
    /// </summary>
    /// <param name="turnContext"><see cref="ITurnContext"/> for this specific activity.</param>
    /// <returns>True if the bot should continue processing this TokenExchange request.</returns>
    async shouldProcessTokenExchange(turnContext) {
        if (turnContext.activity.name !== tokenExchangeOperationName) {
            throw new Error("Only 'signin/tokenExchange' invoke activities can be procssed by TokenExchangeHelper.");
        }

        if (!await this.exchangedToken(turnContext)) {
            // If the TokenExchange is NOT successful, the response will have already been sent by ExchangedToken
            return false;
        }

        // If a user is signed into multiple Teams clients, the Bot might receive a "signin/tokenExchange" from each client.
        // Each token exchange request for a specific user login will have an identical activity.value.Id.
        // Only one of these token exchange requests should be processe by the bot.  For a distributed bot in production,
        // this requires a distributed storage to ensure only one token exchange is processed.

        // This example utilizes Bot Framework IStorage's ETag implementation for token exchange activity deduplication.

        // Create a StoreItem with Etag of the unique 'signin/tokenExchange' request
        const storeItem = {
            eTag: turnContext.activity.value.id
        };

        const storeItems = { [this.getStorageKey(turnContext)]: storeItem };
        try {
            this.storage.write(storeItems);
        }
        catch (err) {
            console.log(err);

            if (err instanceof Error && err.message.startsWith('Etag conflict')) {
                // TODO: Should send 200 invoke response here???
                return false;
            }

            throw err;
        }

        return true;
    }

async exchangedToken(turnContext) {
    let tokenExchangeResponse = null;
    const tokenExchangeRequest = turnContext.activity.value;

    try {
        // ✅ Get UserTokenClient from the CloudAdapter turn state
        const userTokenClient = turnContext.turnState.get(turnContext.adapter.UserTokenClientKey);

        if (!userTokenClient) {
            throw new Error('UserTokenClient not found in turn state.');
        }

        tokenExchangeResponse = await userTokenClient.exchangeToken(
            turnContext.activity.from.id,
            tokenExchangeRequest.connectionName,
            turnContext.activity.channelId,
            { token: tokenExchangeRequest.token }
        );

        console.log('tokenExchangeResponse:', JSON.stringify(tokenExchangeResponse));
    } catch (err) {
        console.error('Token exchange error:', err);
        // Continue gracefully
    }

    if (!tokenExchangeResponse || !tokenExchangeResponse.token) {
        await turnContext.sendActivity({
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
        // ✅ Store in turn state for use in OAuthPrompt
        turnContext.turnState.tokenExchangeInvokeRequest = tokenExchangeRequest;
        turnContext.turnState.tokenResponse = tokenExchangeResponse;
        return true;
    }
}
   
    getStorageKey(turnContext) {

        if (!turnContext || !turnContext.activity || !turnContext.activity.conversation) {
            throw new Error('Invalid context, can not get storage key!');
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