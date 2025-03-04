// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, TextPrompt } = require('botbuilder-dialogs');
const { CardFactory, MessageFactory, InputHints } = require('botbuilder');
const { LogoutDialog } = require('./logoutDialog');
const { SsoOAuthPrompt } = require('./ssoOAuthPrompt');
const { SimpleGraphClient } = require('../simpleGraphClient');

const SSOAUTH = 'SsoAuth';
const OAUTH_PROMPT = 'OAuthPrompt';
const TEXT_PROMPT = 'textPrompt';
let token;

/**
 * BotSSOAuthDialog class handles the SSO authentication process.
 */
class BotSSOAuthDialog extends LogoutDialog {
    constructor(id) {
        super(id, process.env.connectionName);

        this.addDialog(new SsoOAuthPrompt(OAUTH_PROMPT, {
            connectionName: process.env.connectionName,
            text: 'Please Sign In',
            title: 'Sign In',
            timeout: 300000
        }));
        this.addDialog(new TextPrompt(TEXT_PROMPT));
        this.addDialog(new WaterfallDialog(SSOAUTH, [
            this.promptStep.bind(this),
            this.loginStep.bind(this),
            this.userInfoStep.bind(this)
        ]));
        this.initialDialogId = SSOAUTH;
    }

    /**
     * Initiates the OAuth prompt.
     * @param {Object} stepContext - The context of the current step.
     */
    async promptStep(stepContext) {
        return await stepContext.beginDialog(OAUTH_PROMPT);
    }

    /**
     * Handles the login step after the OAuth prompt.
     * @param {Object} stepContext - The context of the current step.
     */
    async loginStep(stepContext) {
        if (stepContext.context._activity.text === "sso") {
            const tokenResponse = stepContext.result;

            if (!tokenResponse || !tokenResponse.token) {
                await stepContext.context.sendActivity('Login was not successful, please try again.');
            } else {
                token = tokenResponse.token;
                const messageText = 'What is your user name?';
                const msg = MessageFactory.text(messageText, messageText, InputHints.ExpectingInput);
                await stepContext.context.sendActivity('Login successful.');
                return await stepContext.prompt(TEXT_PROMPT, { prompt: msg });
            }
            return await stepContext.endDialog();
        }
    }

    /**
     * Retrieves and displays user information after login.
     * @param {Object} stepContext - The context of the current step.
     */
    async userInfoStep(stepContext) {
        const userName = stepContext.result;
        const client = new SimpleGraphClient(token);
        let imageString = "";
        let img2 = "";

        try {
            const myDetails = await client.getMeAsync();
            if (myDetails) {
                const userImage = await client.getUserPhoto();
                const result = await userImage.arrayBuffer();
                console.log(userImage.type);
                imageString = Buffer.from(result).toString('base64');
                img2 = `data:image/png;base64,${imageString}`;

                const userCard = CardFactory.adaptiveCard(this.getAdaptiveCardUserDetails(myDetails, img2, userName));
                await stepContext.context.sendActivity({ attachments: [userCard] });
            }
        } catch (error) {
            console.log(error);
        }

        return await stepContext.endDialog();
    }

    /**
     * Generates an adaptive card with user details.
     * @param {Object} myDetails - The user's details.
     * @param {string} userImage - The user's image in base64 format.
     * @param {string} userName - The user's name.
     * @returns {Object} - The adaptive card JSON.
     */
    getAdaptiveCardUserDetails(myDetails, userImage, userName) {
        return {
            $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
            body: [
                {
                    type: "TextBlock",
                    size: "Medium",
                    weight: "Bolder",
                    text: "User profile details are"
                },
                {
                    type: "Image",
                    size: "Medium",
                    url: userImage
                },
                {
                    type: "TextBlock",
                    size: "Medium",
                    weight: "Bolder",
                    wrap: true,
                    text: `Hello! ${myDetails.displayName}`
                },
                {
                    type: "TextBlock",
                    size: "Medium",
                    weight: "Bolder",
                    text: `Job title: ${myDetails.jobDetails ? myDetails.jobDetails : "Unknown"}`
                },
                {
                    type: "TextBlock",
                    size: "Medium",
                    weight: "Bolder",
                    text: `Email: ${myDetails.userPrincipalName}`
                },
                {
                    type: "TextBlock",
                    size: "Medium",
                    weight: "Bolder",
                    text: `User name: ${userName}`
                }
            ],
            type: 'AdaptiveCard',
            version: '1.4'
        };
    }
}

exports.BotSSOAuthDialog = BotSSOAuthDialog;