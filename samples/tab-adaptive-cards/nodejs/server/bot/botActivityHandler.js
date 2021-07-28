// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler } = require('botbuilder');
const adaptiveCards = require('../models/adaptiveCard');
const { GraphClient } = require('../graphClient')

class BotActivityHandler extends TeamsActivityHandler {
    constructor() {
        super();
    }

    async onInvokeActivity(context) {
        console.log('Activity: ', context.activity.name);

        if (context.activity.name === 'tab/fetch') {

            // When the Bot Service Auth flow completes, context will contain a magic code used for verification.
            const magicCode =
                context.activity.value && context.activity.value.state
                    ? context.activity.value.state
                    : '';

            // Getting the tokenResponse for the user
            const tokenResponse = await context.adapter.getUserToken(
                context,
                process.env.connectionName,
                magicCode
            );

            if (!tokenResponse || !tokenResponse.token) {
                // Token is not available, hence we need to send back the auth response

                // Retrieve the OAuth Sign in Link.
                const signInLink = await context.adapter.getSignInLink(
                    context,
                    process.env.ConnectionName
                );

                // Generating and returning auth response.
                return adaptiveCards.createAuthResponse(signInLink);
            }

            const graphClient = new GraphClient(tokenResponse.token);

            const profile = await graphClient.GetUserProfile().catch(error => {
                console.log(error);
            });

            const userImage = await graphClient.GetUserPhoto().catch(error => {
                console.log(error);
            });

            return adaptiveCards.createFetchResponse(userImage, profile.displayName);
        } else if (context.activity.name === 'tab/submit') {
            console.log('Trying to submit tab content');

            const adapter = context.adapter;
            await adapter.signOutUser(context, process.env.ConnectionName);

            // Generating and returning submit response.
            return adaptiveCards.createSubmitResponse();
        } else if (context.activity.name === "task/fetch") {
            // Task Module task/fetch
            return adaptiveCards.invokeTaskResponse();
        } else if (context.activity.name === "task/submit") {
            // Task Module task/submit
            return adaptiveCards.taskSubmitResponse();
        } 
    }
}

module.exports.BotActivityHandler = BotActivityHandler;