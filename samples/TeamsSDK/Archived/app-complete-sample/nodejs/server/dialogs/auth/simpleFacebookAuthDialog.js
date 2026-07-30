// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ConfirmPrompt, OAuthPrompt, WaterfallDialog } = require('botbuilder-dialogs');

const { LogoutDialog } = require('./logoutDialog');
const axios = require('axios')
const CONFIRM_PROMPT = 'ConfirmPrompt';
const FACEBOOKAUTH = 'FacebookAuth';
const OAUTH_PROMPT = 'OAuthPrompt';

class SimpleFacebookAuthDialog extends LogoutDialog {
    constructor(id, conversationDataAccessor) {
        super(id, process.env.ConnectionName);
        this.conversationDataAccessor = conversationDataAccessor;

        this.addDialog(new OAuthPrompt(OAUTH_PROMPT, {
            connectionName: process.env.ConnectionName,
            text: 'Login to facebook',
            title: 'Log In',
            timeout: 300000
        }));

        this.addDialog(new ConfirmPrompt(CONFIRM_PROMPT));

        this.addDialog(new WaterfallDialog(FACEBOOKAUTH, [
            this.promptStep.bind(this),
            this.loginStep.bind(this),
            this.displayTokenPhase1.bind(this),
            this.displayTokenPhase2.bind(this)
        ]));
        this.initialDialogId = FACEBOOKAUTH;
    }

    async promptStep(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "FacebookAuthDialog";
        return await stepContext.beginDialog(OAUTH_PROMPT);
    }

    async loginStep(stepContext) {

        // Getting the token from the previous step. 
        const tokenResponse = stepContext.result;
        if (tokenResponse) {
            var facbookProfile = await this.getFacebookUserData(tokenResponse.token);
            await stepContext.context.sendActivity('You are now logged in -' +  facbookProfile.first_name);
            return await stepContext.prompt(CONFIRM_PROMPT, 'Would you like to view your token?');
        }
        await stepContext.context.sendActivity('Login was not successful please try again.');
        return await stepContext.endDialog();
    }

    async displayTokenPhase1(stepContext) {
        await stepContext.context.sendActivity('Thank you.');

        const result = stepContext.result;
        if (result) {
            // Call the prompt again because we need the token. The reasons for this are:
            // 1. If the user is already logged in we do not need to store the token locally in the bot and worry
            // about refreshing it. We can always just call the prompt again to get the token.
            // 2. We never know how long it will take a user to respond. By the time the
            // user responds the token may have expired. The user would then be prompted to login again.
            return await stepContext.beginDialog(OAUTH_PROMPT);
        }
        return await stepContext.endDialog();
    }

    // Method to show token.
    async displayTokenPhase2(stepContext) {
        const tokenResponse = stepContext.result;
        if (tokenResponse) {
            await stepContext.context.sendActivity(`Here is your token ${tokenResponse.token}`);
        }
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