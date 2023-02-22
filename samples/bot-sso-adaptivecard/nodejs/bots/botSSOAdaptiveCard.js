// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory, ActivityHandler } = require('botbuilder');
const AdaptiveCardResponse = require('../resources/adaptiveCardResponseJson.json');
const AdaptiveCardWithSSO = require('../resources/AdaptiveCardWithSSOInRefresh.json');
const Options = require('../resources/options.json');
var ACData = require("adaptivecards-templating");
const path = require('path');
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const _connectionName = process.env.ConnectionName;
const uuid = require('uuid');

class BotSSOAdativeCard extends TeamsActivityHandler {
    constructor() {
        super();

        this.onMembersAdded(async (context, next) => {
            //Send welcome message when app installed
            await this.sendWelcomeMessage(context);

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });

        this.onMessage(async (context, next) => {
            const text = context.activity.text;

            if (text) {
                switch (text) {
                    case "login":
                        await context.sendActivity({ attachments: [this.getInitialAdaptiveCard(),] });
                        break;

                    case "PerformSSO":
                        await context.sendActivity({ attachments: [this.getAdaptiveCardWithSSO()] });
                        break;

                    default:
                        const message = "Please send 'login' for options";
                        await context.sendActivity(message);
                        break;
                }
            }
            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }

    /**
    * Invokes when Action.Submit button performs onclick action. 
    * @param {turnContext} context
    */
    async onInvokeActivity(context) {
        if (context._activity.name == "adaptiveCard/action") {
            if (context._activity.value == null) {
                return null;
            }

            var value = context._activity.value;

            if (value["action"] == null) {
                return null;
            }

            var actiondata = value["action"];

            if (actiondata["verb"] == null) {
                return null;
            }

            // authToken and state are absent, handle verb
            let verb = actiondata["verb"];

            var authentication = null;

            // When adaptiveCard / action invoke activity from teams contains token in response to sso flow from earlier invoke.
            if (value["authentication"] != null) {
                authentication = value["authentication"];
            }

            var state = null;
            // when adaptiveCard/action invoke activity from teams contains 6 digit state in response to nominal sign in flow from bot 
            if (value["state"] != null) {
                state = value["state"].toString();
            }

            if (authentication == null && state == null) {
                // authToken is absent, handle verb
                switch (verb) {
                    case "initiateSSO":
                        //when token is absent in the invoke.
                        return this.onInitiateSSO(context);
                }
            }
            else {
                // Send adaptive card with login successful information
                return this.createAdaptiveCardInvokeResponseAsync(authentication,state,context);
            }
        }

        return null;
    }

    /**
    * AuthToken present. Verify token in invoke payload and return AC response
    * @param {authentication} authentication
    * * @param {state} state
    */
    async createAdaptiveCardInvokeResponseAsync(authentication,state) {
        // declare variables
        let isTokenPresent = null;
        let isStatePresent = null;
        let isBasicRefresh = false;
       
        // verify token is present or not
        if (authentication != null) {
            isTokenPresent = true;
        }

        // verify State is present or not
        if (state != undefined || state != "") {
            isStatePresent = true
        }

        // Create a Template instance from the template payload
        var template = new ACData.Template(AdaptiveCardResponse);

        // Use token or state to perform operation on behalf of user
        var authResultData = isTokenPresent ? "SSO success" : isStatePresent ? "OAuth success" : "SSO/OAuth failed"
        if (isBasicRefresh) {
            authResultData = "Refresh done";
        }

        var payloadData = {
            authresult: authResultData
        };

        var card = template.expand(payloadData);
        console.log("card", card);

        var adaptiveCardResponse = {
            StatusCode: 200,
            Type: "application/vnd.microsoft.card.adaptive",
            Value: card
        }

        await ActivityHandler.createInvokeResponse({
            adaptiveCardResponse
        });
    }

    /**
   * Sends OAuth Card with signlink link
   * @param {context} context
   */
    async onInitiateSSO(context) {
        // Retrieve the OAuth Sign in Link
        const signInLink = await context.adapter.getSignInLink(
            context,
            _connectionName
        );

        var oAuthCard = {
            contentType: 'application/vnd.microsoft.card.oauth',
            content: {
                buttons: [{
                    type: 'openUrl',
                    title: 'Please sign in',
                    value: signInLink
                }],
                connectionName: _connectionName,
                tokenExchangeResource: {
                    id: uuid.v1(),
                    uri: null,
                    providerId: null
                },
                tokenPostResource: null,
                text: 'SignIn Text'
            }
        }

        var loginReqResponse = {
            StatusCode: 401,
            Type: "application/vnd.microsoft.card.loginRequest",
            Value: oAuthCard
        }

        return ActivityHandler.createInvokeResponse({
            loginReqResponse
        });
    }

    /**
    * Send a welcome message.
    * @param {TurnContext} turnContext A TurnContext instance containing all the data needed for processing this conversation turn.
    */
    async sendWelcomeMessage(turnContext) {
        const { activity } = turnContext;
        // Iterate over all new members added to the conversation.
        for (const idx in activity.membersAdded) {
            if (activity.membersAdded[idx].id !== activity.recipient.id) {
                const welcomeMessage = `Welcome to Universal Adaptive Cards. Type 'login' to get sign in universal sso.`;

                await turnContext.sendActivity(welcomeMessage);
            }
        }
    }

    /**
    * Sends initial adaptivecard when text 'login' send to bot. 
    */
    getInitialAdaptiveCard() {
        return CardFactory.adaptiveCard(Options);
    }

    /**
    * Sends adaptivecard with Authentication. 
    */
    getAdaptiveCardWithSSO() {
        return CardFactory.adaptiveCard(AdaptiveCardWithSSO);
    }
}

module.exports.BotSSOAdativeCard = BotSSOAdativeCard;
