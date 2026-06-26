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

        // Getting the token from the previous step. 
        const tokenResponse = stepContext.result;
        if (tokenResponse) {
            token = tokenResponse.token;
            const messageText = 'What is your user name?';
            const msg = MessageFactory.text(messageText, messageText, InputHints.ExpectingInput);
            await stepContext.context.sendActivity('Login successful.');
            return await stepContext.prompt(TEXT_PROMPT, { prompt: msg });
        }
        
        await stepContext.context.sendActivity('Login was not successful please try again.');
        return await stepContext.endDialog();
    }

 async userInfoStep(stepContext) {
   const userName = stepContext.result
   var facbookProfile = await this.getFacebookUserData(token);
        const profileCard = CardFactory.adaptiveCard({
            version: '1.0.0',
            type: 'AdaptiveCard',
            body: [
                {
                    type: "Image",
                    size: "Medium",
                    url: facbookProfile.picture.data.url
                },
                {
                    type: 'TextBlock',
                    text: 'Hello: ' + facbookProfile.name,
                },
                {
                    type: 'TextBlock',
                    text: 'User name ' + userName,
                },
            ],
        });
    await stepContext.context.sendActivity({attachments:[profileCard]});

    return await stepContext.endDialog();
 }
    // Method to get facebook user data.
    async getFacebookUserData(access_token) {
        const { data } = await axios({
            url: 'https://graph.facebook.com/v2.6/me',
            method: 'get',
            params: {
                fields: ['name','picture'].join(','),
                access_token: access_token,
            },
        });
        return data;
    };
}

exports.SimpleFacebookAuthDialog = SimpleFacebookAuthDialog;