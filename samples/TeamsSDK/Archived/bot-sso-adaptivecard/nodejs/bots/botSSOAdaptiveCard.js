// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const crypto = require('crypto');
const { TeamsActivityHandler, CardFactory, ActivityHandler } = require('botbuilder');
const ACData = require('adaptivecards-templating');
const AdaptiveCardResponse = require('../resources/adaptiveCardResponseJson.json');
const AdaptiveCardWithSSO = require('../resources/AdaptiveCardWithSSOInRefresh.json');
const Options = require('../resources/options.json');

const _connectionName = process.env.ConnectionName;

class BotSSOAdativeCard extends TeamsActivityHandler {
    constructor() {
        super();

        this.onMembersAdded(async (context, next) => {
            await this.sendWelcomeMessage(context);
            await next();
        });

        this.onMessage(async (context, next) => {
            const text = context.activity.text;

            if (text) {
                switch (text) {
                    case 'login':
                        await context.sendActivity({ attachments: [this.getInitialAdaptiveCard()] });
                        break;

                    case 'PerformSSO':
                        await context.sendActivity({ attachments: [this.getAdaptiveCardWithSSO()] });
                        break;

                    default:
                        await context.sendActivity("Please send 'login' for options");
                        break;
                }
            }

            await next();
        });
    }

    /**
    * Handles adaptiveCard/action invoke activities.
    * @param {TurnContext} context
    */
    async onInvokeActivity(context) {
        if (context.activity.name === 'adaptiveCard/action') {
            const value = context.activity.value;
            if (value == null) {
                return null;
            }

            const actiondata = value.action;
            if (actiondata == null || actiondata.verb == null) {
                return null;
            }

            const verb = actiondata.verb;
            const authentication = value.authentication || null;
            const state = value.state != null ? value.state.toString() : null;

            if (authentication == null && state == null) {
                switch (verb) {
                    case 'initiateSSO':
                        return this.onInitiateSSO(context);
                }
            } else {
                return this.createAdaptiveCardInvokeResponseAsync(authentication, state);
            }
        }

        return null;
    }

    /**
    * Verifies token/state in invoke payload and returns adaptive card response.
    * @param {object} authentication - The authentication object from the invoke payload
    * @param {string} state - The state value from the invoke payload
    */
    createAdaptiveCardInvokeResponseAsync(authentication, state) {
        const isTokenPresent = authentication != null;
        const isStatePresent = state != null && state !== '';

        const template = new ACData.Template(AdaptiveCardResponse);

        const authResultData = isTokenPresent ? 'SSO success' : isStatePresent ? 'OAuth success' : 'SSO/OAuth failed';

        const card = template.expand({
            authresult: authResultData
        });

        const adaptiveCardResponse = {
            statusCode: 200,
            type: 'application/vnd.microsoft.card.adaptive',
            value: card
        };

        return ActivityHandler.createInvokeResponse(adaptiveCardResponse);
    }

    /**
    * Sends OAuth Card with sign-in link to initiate SSO flow.
    * @param {TurnContext} context
    */
    async onInitiateSSO(context) {
        const signInLink = await context.adapter.getSignInLink(
            context,
            _connectionName
        );

        const oAuthCard = {
            contentType: 'application/vnd.microsoft.card.oauth',
            content: {
                buttons: [{
                    type: 'openUrl',
                    title: 'Please sign in',
                    value: signInLink
                }],
                connectionName: _connectionName,
                tokenExchangeResource: {
                    id: crypto.randomUUID(),
                    uri: null,
                    providerId: null
                },
                tokenPostResource: null,
                text: 'SignIn Text'
            }
        };

        const loginReqResponse = {
            statusCode: 401,
            type: 'application/vnd.microsoft.activity.loginRequest',
            value: oAuthCard
        };

        return ActivityHandler.createInvokeResponse(loginReqResponse);
    }

    /**
    * Send a welcome message to new members.
    * @param {TurnContext} turnContext
    */
    async sendWelcomeMessage(turnContext) {
        const { activity } = turnContext;
        for (const idx in activity.membersAdded) {
            if (activity.membersAdded[idx].id !== activity.recipient.id) {
                await turnContext.sendActivity('Welcome to Universal Adaptive Cards. Type \'login\' to get sign in universal sso.');
            }
        }
    }

    getInitialAdaptiveCard() {
        return CardFactory.adaptiveCard(Options);
    }

    getAdaptiveCardWithSSO() {
        return CardFactory.adaptiveCard(AdaptiveCardWithSSO);
    }
}

module.exports.BotSSOAdativeCard = BotSSOAdativeCard;
