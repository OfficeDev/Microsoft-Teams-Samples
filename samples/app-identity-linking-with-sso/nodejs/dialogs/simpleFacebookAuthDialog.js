// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { OAuthPrompt, WaterfallDialog } = require('botbuilder-dialogs');
const { MessageFactory } = require('botbuilder');
const { LogoutDialog } = require('./logoutDialog');
const CardHelper = require('../cards/cardHelper');
const Data = require('../helper/dataHelper');
const FACEBOOKAUTH = 'FacebookAuth';
const OAUTH_PROMPT = 'OAuthPrompt';
var userDetailsList = require('./mainDialog');

class SimpleFacebookAuthDialog extends LogoutDialog {
  constructor(id) {
    super(id, process.env.FBConnectionName);

    this.addDialog(new OAuthPrompt(OAUTH_PROMPT, {
      connectionName: process.env.FBConnectionName,
      text: 'Login to facebook',
      title: 'Log In',
      timeout: 300000
    }));

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
      var facbookProfile = await Data.getFacebookUserData(tokenResponse.token);
      var userData;
      var currentData = userDetailsList.userDetails["userDetails"];
      let updateindex;
      currentData.map((user, index) => {
        if (user.aad_id == stepContext.context._activity.from.aadObjectId) {
          updateindex = index;
          userData = user;
          userData['facebook_id'] = facbookProfile.id;
          userData['facebook_token'] = tokenResponse.token;
          userData['is_fb_signed_in'] = true;
        }
      })
      currentData[updateindex] = userData;
      userDetailsList.userDetails["userDetails"] = currentData;

      var aadProfile = await Data.getAADUserData(userData.aad_token);
      var aadDetailCard = CardHelper.getAADDetailsCard(aadProfile.myDetails, aadProfile.photo);
      var facebookdetailCard = CardHelper.getFacebookDetailsCard(facbookProfile);
      var googleDetailCard;

      if (userData.is_google_signed_in) {
        var googleProfile = await Data.getGoogleUserData(userData.google_token);
        googleDetailCard = CardHelper.getGoogleDetailsCard(googleProfile)
      }
      else {
        googleDetailCard = CardHelper.getConnectToGoogleCard();
      }
      await stepContext.context.sendActivity(MessageFactory.list([aadDetailCard, facebookdetailCard, googleDetailCard]));
      
      return await stepContext.endDialog();
    }
    await stepContext.context.sendActivity('Login was not successful please try again.');
    
    return await stepContext.endDialog();
  }
}

exports.SimpleFacebookAuthDialog = SimpleFacebookAuthDialog;