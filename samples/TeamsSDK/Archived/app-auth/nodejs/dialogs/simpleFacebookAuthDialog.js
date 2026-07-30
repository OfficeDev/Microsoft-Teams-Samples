// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { OAuthPrompt, WaterfallDialog,TextPrompt} = require('botbuilder-dialogs');
const { CardFactory,MessageFactory,InputHints} = require('botbuilder');

const { LogoutDialog } = require('./logoutDialog');
const axios = require('axios')
const FACEBOOKAUTH = 'FacebookAuth';
const OAUTH_PROMPT = 'OAuthPrompt';
const TEXT_PROMPT = 'textPrompt';
var  token;

class SimpleFacebookAuthDialog extends LogoutDialog {
    constructor(id) {
        super(id, process.env.FBConnectionName);

        this.addDialog(new OAuthPrompt(OAUTH_PROMPT, {
            connectionName: process.env.FBConnectionName,
            text: 'Login to facebook',
            title: 'Log In',
            timeout: 300000
        }));
        
        this.addDialog(new TextPrompt(TEXT_PROMPT));

        this.addDialog(new WaterfallDialog(FACEBOOKAUTH, [
            this.promptStep.bind(this),
            this.loginStep.bind(this),
            this.userInfoStep.bind(this)
        ]));

        this.initialDialogId = FACEBOOKAUTH;
    }

    async promptStep(stepContext) {
        return await stepContext.beginDialog(OAUTH_PROMPT);
    }

    async loginStep(stepContext) {
        const tokenResponse = stepContext.result;
        
        if (tokenResponse) {
            token = tokenResponse.token;
            await stepContext.context.sendActivity('Login successful.');
            return await this.userInfoStep(stepContext);
        }
        
        await stepContext.context.sendActivity('Login was not successful. Please try again.');
        return await stepContext.endDialog();
    }

    async userInfoStep(stepContext) {
        try {
            const facebookProfile = await this.getFacebookUserData(token);

            const profileCard = CardFactory.adaptiveCard({
                version: '1.0.0',
                type: 'AdaptiveCard',
                body: [
                    {
                        type: "Image",
                        size: "Medium",
                        url: facebookProfile.picture.data.url
                    },
                    {
                        type: 'TextBlock',
                        text: `Hello: ${facebookProfile.name}`
                    }
                ]
            });

            await stepContext.context.sendActivity({ attachments: [profileCard] });
        } catch (error) {
            console.error('Error retrieving Facebook profile:', error.message);
            await stepContext.context.sendActivity('Unable to retrieve profile information.');
        }

        return await stepContext.endDialog();
    }

    async getFacebookUserData(accessToken) {
        try {
            const { data } = await axios({
                url: 'https://graph.facebook.com/v2.6/me',
                method: 'get',
                params: {
                    fields: ['name', 'picture'].join(','),
                    access_token: accessToken
                }
            });
            return data;
        } catch (error) {
            console.error('Error fetching Facebook user data:', error.message);
            throw error;
        }
    }
}

exports.SimpleFacebookAuthDialog = SimpleFacebookAuthDialog;