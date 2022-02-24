// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ConfirmPrompt, OAuthPrompt, WaterfallDialog } = require('botbuilder-dialogs');
const { CardFactory} = require('botbuilder');

const { LogoutDialog } = require('./logoutDialog');
const axios = require('axios')
const GOOGLEAUTH = 'GoogleAuth';
const OAUTH_PROMPT = 'OAuthPrompt';

class GoogleAuthDialog extends LogoutDialog {
    constructor(id) {
        super(id, process.env.GoogleConnectionName);

        this.addDialog(new OAuthPrompt(OAUTH_PROMPT, {
            connectionName: process.env.GoogleConnectionName,
            text: 'Login to Google',
            title: 'Log In',
            timeout: 300000
        }));

        this.addDialog(new WaterfallDialog(GOOGLEAUTH, [
            this.promptStep.bind(this),
            this.loginStep.bind(this)
        ]));
        this.initialDialogId = GOOGLEAUTH;
    }

    async promptStep(stepContext) {
        return await stepContext.beginDialog(OAUTH_PROMPT);
    }

    async loginStep(stepContext) {

        // Getting the token from the previous step. 
        const tokenResponse = stepContext.result;
        if (tokenResponse) {
            var googleProfile = await this.getFacebookUserData(tokenResponse.token);
            const profileCard = CardFactory.adaptiveCard({
                version: '1.0.0',
                type: 'AdaptiveCard',
                body: [
                    {
                        type: "Image",
                        size: "Medium",
                        url: googleProfile.photos[0].url
                    },
                    {
                        type: 'TextBlock',
                        text: 'Hello: ' + googleProfile.names[0].displayName,
                    },
                    {
                        type: 'TextBlock',
                        text: 'Email: ' + googleProfile.emailAddresses[0].value,
                    },
                ],
            });
            await stepContext.context.sendActivity({attachments:[profileCard]});
            return await stepContext.endDialog();
        }
        await stepContext.context.sendActivity('Login was not successful please try again.');
        return await stepContext.endDialog();
    }

    // Method to get facebook user data.
    async getFacebookUserData(access_token) {
        const data  = await axios.get('https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls', {
                        headers: {
                            "Authorization": `Bearer ${access_token}`,
                        }
                        })

        return data.data;
    };
}

exports.GoogleAuthDialog = GoogleAuthDialog;