// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { OAuthPrompt, WaterfallDialog } = require('botbuilder-dialogs');
const { MessageFactory } = require('botbuilder');
const CardHelper = require('../cards/cardHelper');
const { LogoutDialog } = require('./logoutDialog');
const Data = require('../helper/dataHelper');
const GOOGLEAUTH = 'GoogleAuth';
const OAUTH_PROMPT = 'OAuthPrompt';
var userDetailsList = require('./mainDialog');

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
            var googleProfile = await Data.getGoogleUserData(tokenResponse.token);
            var userData;
            var currentData = userDetailsList.userDetails["userDetails"];
            let updateindex;
            currentData.map((user, index) => {
                if (user.aad_id == stepContext.context._activity.from.aadObjectId) {
                    updateindex = index;
                    userData = user;
                    userData['google_id'] = googleProfile.emailAddresses[0].value;
                    userData['google_token'] = tokenResponse.token;
                    userData['is_google_signed_in'] = true;
                }
            })
            currentData[updateindex] = userData;
            userDetailsList.userDetails["userDetails"] = currentData;
            var aadProfile = await Data.getAADUserData(userData.aad_token);
            var aadDetailCard = CardHelper.getAADDetailsCard(aadProfile.myDetails, aadProfile.photo);
            var googleDetailsCard = CardHelper.getGoogleDetailsCard(googleProfile);
            var facebookdetailCard;
            
            if (userData.is_fb_signed_in) {
                var facebookProfile = await Data.getFacebookUserData(userData.facebook_token);
                facebookdetailCard = CardHelper.getFacebookDetailsCard(facebookProfile)
            }
            else {
                facebookdetailCard = CardHelper.getConnectToFacebookCard();
            }

            await stepContext.context.sendActivity(MessageFactory.list([aadDetailCard, facebookdetailCard, googleDetailsCard]));

            return await stepContext.endDialog();
        }

        await stepContext.context.sendActivity('Login was not successful please try again.');
        
        return await stepContext.endDialog();
    }
}

exports.GoogleAuthDialog = GoogleAuthDialog;