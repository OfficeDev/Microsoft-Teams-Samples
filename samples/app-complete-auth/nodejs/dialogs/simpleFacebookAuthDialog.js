// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ConfirmPrompt, OAuthPrompt, WaterfallDialog } = require('botbuilder-dialogs');

const { LogoutDialog } = require('./logoutDialog');
const axios = require('axios')
const CONFIRM_PROMPT = 'ConfirmPrompt';
const FACEBOOKAUTH = 'FacebookAuth';
const OAUTH_PROMPT = 'OAuthPrompt';

class SimpleFacebookAuthDialog extends LogoutDialog {
    constructor(id) {
        super(id, process.env.FBConnectionName);

        this.addDialog(new OAuthPrompt(OAUTH_PROMPT, {
            connectionName: process.env.FBConnectionName,
            text: 'Login to facebook',
            title: 'Log In',
            timeout: 300000
        }));

        this.addDialog(new ConfirmPrompt(CONFIRM_PROMPT));

        this.addDialog(new WaterfallDialog(FACEBOOKAUTH, [
            this.promptStep.bind(this),
            this.loginStep.bind(this)
        ]));
        this.initialDialogId = FACEBOOKAUTH;
    }

    async promptStep(stepContext) {
        return await stepContext.beginDialog(OAUTH_PROMPT);
    }

    async loginStep(stepContext) {

        // Getting the token from the previous step. 
        const tokenResponse = stepContext.result;
        if (tokenResponse) {
            var facbookProfile = await this.getFacebookUserData(tokenResponse.token);
            await stepContext.context.sendActivity('You are now logged in -' + facbookProfile.first_name);
            return await stepContext.endDialog();
        }
        await stepContext.context.sendActivity('Login was not successful please try again.');
        return await stepContext.endDialog();
    }

    // Method to get facebook user data.
    async getFacebookUserData(access_token) {
        const { data } = await axios({
            url: 'https://graph.facebook.com/me',
            method: 'get',
            params: {
                fields: ['id', 'email', 'first_name', 'last_name'].join(','),
                access_token: access_token,
            },
        });
        console.log(data); // { id, email, first_name, last_name }
        return data;
    };
}

exports.SimpleFacebookAuthDialog = SimpleFacebookAuthDialog;