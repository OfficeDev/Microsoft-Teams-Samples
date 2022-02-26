// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { OAuthPrompt, WaterfallDialog } = require('botbuilder-dialogs');
const { MessageFactory, ActionTypes } = require('botbuilder');
const { SimpleGraphClient } = require('../simpleGraphClient');

const { LogoutDialog } = require('./logoutDialog');
const axios = require('axios')
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
            var googleProfile = await this.getGoogleUserData(tokenResponse.token);
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
            var aadProfile = await this.getAADUserData(userData.aad_token);
            var aadDetailCard = this.getAADDetailsCard(aadProfile.myDetails, aadProfile.photo);
            var googleDetailsCard = this.getGoogleDetailsCard(googleProfile);
            var facebookdetailCard;
            if (userData.is_fb_signed_in) {
                var facebookProfile = await this.getFacebookUserData(userData.facebook_token);
                facebookdetailCard = this.getFacebookDetailsCard(facebookProfile)
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

            await stepContext.context.sendActivity(MessageFactory.list([aadDetailCard, facebookdetailCard, googleDetailsCard]));

            return await stepContext.endDialog();
        }

        await stepContext.context.sendActivity('Login was not successful please try again.');
        return await stepContext.endDialog();
    }

    // Method to get facebook user data.
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
            "title": 'Hello: ' + facbookProfile.name ,
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
}

exports.GoogleAuthDialog = GoogleAuthDialog;