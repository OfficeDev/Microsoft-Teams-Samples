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

            var profileImageUrl = 'https://cdn.vox-cdn.com/thumbor/Ndb49Uk3hjiquS041NDD0tPDPAs=/0x169:1423x914/fit-in/1200x630/cdn.vox-cdn.com/uploads/chorus_asset/file/7342855/microsoftteams.0.jpg';
            await graphClient.GetUserPhoto().then(result => {
                var imageType = "image/jpeg"
                var imageBytes = Buffer.from(image.data).toString('base64');
                profileImageUrl = `data:${imageType};base64,${imageBytes}`;
                // Generating and returning continue response.
                return adaptiveCards.createFetchResponse(profileImageUrl, context.activity.from.name);
            }).catch((error) => {
                return adaptiveCards.createFetchResponse(profileImageUrl, context.activity.from.name);
            });

        } else if (context.activity.name === 'tab/submit') {
            console.log('Trying to submit tab content');
            // Generating and returning submit response.
            return adaptiveCards.createSubmitResponse();
        } else if (context.activity.name === "task/fetch") {
            return adaptiveCards.invokeTaskResponse();
        }
    }
}

module.exports.BotActivityHandler = BotActivityHandler;