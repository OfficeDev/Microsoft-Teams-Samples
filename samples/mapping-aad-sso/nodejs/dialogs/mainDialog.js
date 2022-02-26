// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogSet, DialogTurnStatus, WaterfallDialog } = require('botbuilder-dialogs');
const { MessageFactory, ActionTypes } = require('botbuilder');
const { LogoutDialog } = require('./logoutDialog');
const MAIN_DIALOG = 'MainDialog';
const MAIN_WATERFALL_DIALOG = 'MainWaterfallDialog';
const OAUTH_PROMPT = 'OAuthPrompt';
const { polyfills } = require('isomorphic-fetch');
const { SsoOAuthPrompt } = require('./ssoOAuthPrompt');
const { SimpleGraphClient } = require('../simpleGraphClient');
const { SimpleFacebookAuthDialog } = require('./simpleFacebookAuthDialog');
const { GoogleAuthDialog } = require('./googleAuthDialog');
const FACEBOOKAUTH = 'FacebookAuth';
const GOOGLEAUTH = 'GoogleAuth';
const axios = require('axios')
const userDetails = {};

class MainDialog extends LogoutDialog {
  constructor() {
    super(MAIN_DIALOG, process.env.connectionName);
    this.baseUrl = process.env.ApplicationBaseUrl;
    this.addDialog(new SsoOAuthPrompt(OAUTH_PROMPT, {
      connectionName: process.env.connectionName,
      text: 'Please Sign In',
      title: 'Sign In',
      timeout: 300000
    }));

    this.addDialog(new SimpleFacebookAuthDialog(FACEBOOKAUTH));
    this.addDialog(new GoogleAuthDialog(GOOGLEAUTH));

    this.addDialog(new WaterfallDialog(MAIN_WATERFALL_DIALOG, [
      this.promptStep.bind(this),
      this.loginStep.bind(this)
    ]));

    this.initialDialogId = MAIN_WATERFALL_DIALOG;
  }

  /**
  * The run method handles the incoming activity (in the form of a DialogContext) and passes it through the dialog system.
  * If no dialog is active, it will start the default dialog.
  * @param {*} dialogContext
  */
  async run(context, accessor) {
    const dialogSet = new DialogSet(accessor);
    dialogSet.add(this);
    const dialogContext = await dialogSet.createContext(context);
    const results = await dialogContext.continueDialog();

    if (results.status === DialogTurnStatus.empty) {
      await dialogContext.beginDialog(this.id);
    }
  }

  async promptStep(stepContext) {
    try {
      return await stepContext.beginDialog(OAUTH_PROMPT);
    } catch (err) {
      console.error(err);
    }
  }

  async loginStep(stepContext) {
    // Get the token from the previous step. Note that we could also have gotten the
    // token directly from the prompt itself. There is an example of this in the next method.
    const tokenResponse = stepContext.result;
    if (!tokenResponse || !tokenResponse.token) {
      await stepContext.context.sendActivity('Login was not successful please try again.');
      return await stepContext.endDialog();
    }
    else {
      var currentData = userDetails["userDetails"];
      if (currentData == undefined) {
        const userDetailsList = new Array();
        userDetailsList.push({ "aad_id": stepContext.context._activity.from.aadObjectId, "is_aad_signed_in": true, "aad_token": tokenResponse.token });
        currentData = userDetailsList;
        userDetails["userDetails"] = currentData;
        await this.getUserEntryCard(stepContext, tokenResponse.token);
        return await stepContext.endDialog();
      }
      else if (!currentData.find((user) => {
        if (user.aad_id == stepContext.context._activity.from.aadObjectId) {
          return true;
        }
      })) {
        const userDetailsList = currentData;
        userDetailsList.push({ "aad_id": stepContext.context._activity.from.aadObjectId, "is_aad_signed_in": true, "aad_token": tokenResponse.token });
        currentData = userDetailsList;
        userDetails["userDetails"] = currentData;
        await this.getUserEntryCard(stepContext, tokenResponse.token);
        return await stepContext.endDialog();
      }
      else {
        var userData;
        let updateindex;
        currentData.find((user, index) => {
          if (user.aad_id == stepContext.context._activity.from.aadObjectId) {
            userData = user;
            updateindex = index;
          }
        })
        userData["aad_token"] = tokenResponse.token
        currentData[updateindex] = userData;
        userDetails["userDetails"] = currentData;

        var aadProfile = await this.getAADUserData(tokenResponse.token);
        var aadDetailCard = this.getAADDetailsCard(aadProfile.myDetails, aadProfile.photo);
        var facbookProfile;
        var facebookdetailCard;
        var googleDetailsCard;
        var googleProfile;
        if (!userData.is_fb_signed_in) {
          if (stepContext.context._activity.text != undefined && stepContext.context._activity.text == "connectToFacebook") {
            return await stepContext.beginDialog(FACEBOOKAUTH);
          }
          else {
            facebookdetailCard = {
              "contentType": "application/vnd.microsoft.card.hero",
              "content": {
                "buttons": [
                  {
                    "type": ActionTypes.ImBack,
                    "title": "Connect to facebook",
                    "value": "connectToFacebook"
                  }
                ]
              }
            }
          }
        }
        else {
          if (stepContext.context._activity.text != undefined && stepContext.context._activity.text == "DisConnectFromFacebook") {
            userData['facebook_id'] = null;
            userData['facebook_token'] = null;
            userData['is_fb_signed_in'] = false;
            currentData[updateindex] = userData;
            userDetails["userDetails"] = currentData;
            facebookdetailCard = {
              "contentType": "application/vnd.microsoft.card.hero",
              "content": {
                "buttons": [
                  {
                    "type": ActionTypes.ImBack,
                    "title": "Connect to facebook",
                    "value": "connectToFacebook"
                  }
                ]
              }
            }
          }
          else {
            facbookProfile = await this.getFacebookUserData(userData.facebook_token);
            facebookdetailCard = this.getFacebookDetailsCard(facbookProfile);
          }
        }
        if (!userData.is_google_signed_in) {
          if (stepContext.context._activity.text != undefined && stepContext.context._activity.text == "connectToGoogle") {
            return await stepContext.beginDialog(GOOGLEAUTH);
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
        }
        else {
          if (stepContext.context._activity.text != undefined && stepContext.context._activity.text == "DisConnectFromGoogle") {
            userData['google_id'] = null;
            userData['google_token'] = null;
            userData['is_google_signed_in'] = false;
            currentData[updateindex] = userData;
            userDetails["userDetails"] = currentData;
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
          else {
            googleProfile = await this.getGoogleUserData(userData.google_token);
            googleDetailsCard = this.getGoogleDetailsCard(googleProfile)
          }
        }
        await stepContext.context.sendActivity(MessageFactory.list([aadDetailCard, facebookdetailCard, googleDetailsCard]));
        return await stepContext.endDialog();
      }
    }
  }

  async getUserEntryCard(stepContext, token) {
    const data = await this.getAADUserData(token);
    const userCard = MessageFactory.list(this.getAdaptiveCardUserDetails(data.myDetails, data.photo));
    await stepContext.context.sendActivity(userCard);
  }

  getAdaptiveCardUserDetails = (myDetails, userImage) => (
    [this.getAADDetailsCard(myDetails, userImage),
    {
      "contentType": "application/vnd.microsoft.card.hero",
      "content": {
        "buttons": [
          {
            "type": ActionTypes.ImBack,
            "title": "Connect to facebook",
            "value": "connectToFacebook"
          }
        ]
      }
    },
    {
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
    ]);

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

  // Method to get facebook user data.
  async getFacebookUserData(access_token) {
    const { data } = await axios({
      url: 'https://graph.facebook.com/v2.6/me',
      method: 'get',
      params: {
        fields: ['name', 'picture'].join(','),
        access_token: access_token,
      },
    });
    return data;
  };

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
          "title": "Disconnect from facebook",
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

module.exports.MainDialog = MainDialog;

exports.userDetails = userDetails;