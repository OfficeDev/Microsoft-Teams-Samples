// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { OAuthPrompt, WaterfallDialog } = require('botbuilder-dialogs');
const { SimpleGraphClient } = require('../simpleGraphClient');
const { MessageFactory, ActionTypes } = require('botbuilder');
const { LogoutDialog } = require('./logoutDialog');
const axios = require('axios')
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
      var facbookProfile = await this.getFacebookUserData(tokenResponse.token);
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

      var aadProfile = await this.getAADUserData(userData.aad_token);
      var aadDetailCard = this.getAADDetailsCard(aadProfile.myDetails, aadProfile.photo);
      var facebbokdetailCard = this.getFacebookDetailsCard(facbookProfile);
      var googleDetailsCard;
      if (userData.is_google_signed_in) {
        var googleProfile = await this.getGoogleUserData(userData.google_token);
        googleDetailsCard = this.getGoogleDetailsCard(googleProfile)
      }
      else {
        googleDetailsCard = {
          "contentType": "application/vnd.microsoft.card.hero",
          "content": {
            "buttons": [
              {
                "type": ActionTypes.ImBack,
                "title": "Connect to google",
                "value": "connectToGoogle"
              }
            ]
          }
        }
      }
      await stepContext.context.sendActivity(MessageFactory.list([aadDetailCard, facebbokdetailCard, googleDetailsCard]));
      return await stepContext.endDialog();
    }
    await stepContext.context.sendActivity('Login was not successful please try again.');
    return await stepContext.endDialog();
  }

  // Method to get facebook user data.
  async getFacebookUserData(access_token) {
    const { data } = await axios({
      url: 'https://graph.facebook.com/v2.6/me',
      method: 'get',
      params: {
        fields: ['name', 'picture', 'id'].join(','),
        access_token: access_token,
      },
    });
    return data;
  };

  getAADDetailsCard = (myDetails, userImage) => (
    {
      "contentType": "application/vnd.microsoft.card.adaptive",
      "content": {
        "type": "AdaptiveCard",
        "version": "1.0",
        "body": [
          {
            "type": "TextBlock",
            "size": "Medium",
            "weight": "Bolder",
            "text": "User profile details are"
          },
          {
            "type": "Image",
            "size": "Medium",
            "url": userImage
          },
          {
            "type": "TextBlock",
            "size": "Medium",
            "weight": "Bolder",
            "wrap": true,
            "text": `Hello! ${myDetails.displayName}`
          },
          {
            "type": "TextBlock",
            "size": "Medium",
            "weight": "Bolder",
            "text": `Job title: ${myDetails.jobDetails ? myDetails.jobDetails : "Unknown"}`
          },
          {
            "type": "TextBlock",
            "size": "Medium",
            "weight": "Bolder",
            "text": `Email: ${myDetails.userPrincipalName}`
          }
        ]
      }
    });

  async getAADUserData(token) {
    const client = new SimpleGraphClient(token);
    const myDetails = await client.getMeAsync();
    var imageString = "";
    var img2 = "";
    if (myDetails != null) {
      var userImage = await client.getUserPhoto();
      await userImage.arrayBuffer().then(result => {
        imageString = Buffer.from(result).toString('base64');
        img2 = "data:image/png;base64," + imageString;
      }).catch(error => {
        console.log(error)
      });
    }
    return {
      myDetails: myDetails,
      photo: img2
    }
  }

  getFacebookDetailsCard = (facbookProfile) => ({
    "contentType": "application/vnd.microsoft.card.hero",
    "content": {
      "title": 'Hello: ' + facbookProfile.name,
      "images": [
        {
          "url": facbookProfile.picture.data.url
        }
      ],
      "buttons": [
        {
          "type": ActionTypes.ImBack,
          "title": "Disconnect from facebok",
          "value": "DisConnectFromFacebook"
        }
      ]
    }
  });

  async getGoogleUserData(access_token) {
    const data = await axios.get('https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls', {
      headers: {
        "Authorization": `Bearer ${access_token}`,
      }
    })

    return data.data;
  };

  getGoogleDetailsCard = (googleProfile) => ({
    "contentType": "application/vnd.microsoft.card.hero",
    "content": {
      "title": 'Hello: ' + googleProfile.names[0].displayName,
      "subtitle": 'Email: ' + googleProfile.emailAddresses[0].value,
      "images": [
        {
          "url": googleProfile.photos[0].url
        }
      ],
      "buttons": [
        {
          "type": ActionTypes.ImBack,
          "title": "Disconnect from google",
          "value": "DisConnectFromGoogle"
        }
      ]
    }
  });
}

exports.SimpleFacebookAuthDialog = SimpleFacebookAuthDialog;