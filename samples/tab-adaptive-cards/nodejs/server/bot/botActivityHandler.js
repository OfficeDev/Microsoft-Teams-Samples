// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, TurnContext } = require('botbuilder');
const adaptiveCards = require('../models/adaptiveCard');
const { GraphClient } = require('../graphClient')

class BotActivityHandler extends TeamsActivityHandler {
    constructor() {
        super();
    }

    async handleTeamsTabFetch(context, tabRequest) {
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

        if (tabRequest.tabContext.tabEntityId == "youtubeTab") {
            return adaptiveCards.createFetchResponseForTab2();
        }

        else {
            const graphClient = new GraphClient(tokenResponse.token);

            const profile = await graphClient.GetUserProfile().catch(error => {
                console.log(error);
            });

            const userImage = await graphClient.GetUserPhoto().catch(error => {
                console.log(error);
            });

            return adaptiveCards.createFetchResponse(userImage, profile.displayName);
        }
    }

    async handleTeamsTabSubmit(context, tabRequest) {
        console.log('Trying to submit tab content');

        const adapter = context.adapter;
        await adapter.signOutUser(context, process.env.ConnectionName);

        // Generating and returning submit response.
        return adaptiveCards.createSubmitResponse();
    }

    handleTeamsTaskModuleFetch(context, taskModuleRequest) {
        var tabEntityId = taskModuleRequest.tabContext.tabEntityId;
        if (tabEntityId == "youtubeTab") {
            var videoId = taskModuleRequest.data.youTubeVideoId;
            return adaptiveCards.videoInvokeResponse(videoId);
        }
        else
            return adaptiveCards.invokeTaskResponse();
    }

    handleTeamsTaskModuleSubmit(context, taskModuleRequest) {
        return adaptiveCards.taskSubmitResponse();
    }
}

module.exports.BotActivityHandler = BotActivityHandler;