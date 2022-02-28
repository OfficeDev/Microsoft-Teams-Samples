// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogBot } = require('./dialogBot');
const { tokenExchangeOperationName } = require('botbuilder');
const { SsoOAuthHelpler } = require('../SsoOAuthHelpler');
const { CardFactory} = require('botbuilder');
const { SimpleGraphClient } = require('../simpleGraphClient.js');
const Data = require('../helper/dataHelper');
const CardHelper = require('../cards/cardHelper');
const axios = require('axios')
const userDetails = {};
let is_fb_signed_in;
let is_google_signed_in;
class TeamsBot extends DialogBot {
    /**
    *
    * @param {ConversationState} conversationState
    * @param {UserState} userState
    * @param {Dialog} dialog
    */
    constructor(conversationState, userState, dialog) {
        super(conversationState, userState, dialog);
        this._ssoOAuthHelper = new SsoOAuthHelpler(process.env.ConnectionName, conversationState);
        this.connectionName = process.env.ConnectionName;
        this.fbconnectionName = process.env.FBConnectionName;
        this.googleconnectionName = process.env.GoogleConnectionName
        this.baseUrl = process.env.ApplicationBaseUrl;
        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let member = 0; member < membersAdded.length; member++) {
                if (membersAdded[member].id !== context.activity.recipient.id) {
                    await context.sendActivity("Hello and welcome! Please type 'login' for initiating the authentication flow.");
                    await this.dialog.run(context, this.dialogState);
                }
            }

            await next();
        });
    }

    async onTokenResponseEvent(context) {
        console.log('Running dialog with Token Response Event Activity.');

        // Run the Dialog with the new Token Response Event Activity.
        await this.dialog.run(context, this.dialogState);
    }
    
    async handleTeamsSigninVerifyState(context, query) {
        console.log('Running dialog with signin/verifystate from an Invoke Activity.');
        await this.dialog.run(context, this.dialogState);
    }

    async handleTeamsMessagingExtensionSubmitAction(context, action) {
        var state = action.data.msteams.id;
        var currentData = userDetails["userDetails"];
        var userData;
        let updateindex;
        currentData.find((user, index) => {
          if (user.aad_id == context.activity.from.aadObjectId) {
            userData = user;
            updateindex = index;
          }
        })
        var facebookProfile ={};
        var googleProfile={};
        var ssoData = await Data.getAADUserData(userData.aad_token);
        var card;
        if(state == 'connectWithFacebook' || is_fb_signed_in){
            const userTokenClient = context.turnState.get(context.adapter.UserTokenClientKey);
            const magicCode =action.state && Number.isInteger(Number(action.state))? action.state: '';

            const tokenResponse = await userTokenClient.getUserToken(
                context.activity.from.id,
                this.fbconnectionName,
                context.activity.channelId,
                magicCode
            );
        
            if (!tokenResponse || !tokenResponse.token) {
                // There is no token, so the user has not signed in yet.
                // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                is_fb_signed_in = true;
                is_google_signed_in = false;
                const { signInLink } = await userTokenClient.getSignInResource(
                    this.fbconnectionName,
                    context.activity
                );

                return {
                    composeExtension: {
                        type: 'auth',
                        suggestedActions: {
                            actions: [
                                {
                                    type: 'openUrl',
                                    value: signInLink,
                                    title: 'Facebook Oauth'
                                },
                            ],
                        },
                    },
                };
            }

            var facebookProfileDetail = await Data.getFacebookUserData(tokenResponse.token);
            userData['facebook_id'] = facebookProfileDetail.id;
            userData['facebook_token'] = tokenResponse.token;
            userData['is_fb_signed_in'] = true;
            currentData[updateindex] = userData;
            userDetails["userDetails"] = currentData;
            facebookProfile["is_fb_signed_in"]= true;
            facebookProfile["name"]= facebookProfileDetail.name;
            facebookProfile["image"]= facebookProfileDetail.picture.data.url;
            if(userData.is_google_signed_in){
                var googleProfileDetails = await Data.getGoogleUserData(userData.google_token);
                googleProfile["is_google_signed_in"]= true;
                googleProfile["name"] = googleProfileDetails.names[0].displayName;
                googleProfile["image"] = googleProfileDetails.photos[0].url;
                googleProfile["email"] = googleProfileDetails.emailAddresses[0].value;
            }
            else{
                googleProfile["is_google_signed_in"]= false
            }
            card = CardHelper.getMEResponseCard(ssoData.myDetails, ssoData.photo,facebookProfile,googleProfile);
            return {
                task: {
                    type: 'continue',
                    value: {
                        card: card,
                        heigth: 250,
                        width: 400,
                        title: 'Show Profile Card'
                    },
                },
            };
        }
        if(state == 'connectWithGoogle' || is_google_signed_in){
            const userTokenClient = context.turnState.get(context.adapter.UserTokenClientKey);
            const magicCode =action.state && Number.isInteger(Number(action.state))? action.state: '';

            const tokenResponse = await userTokenClient.getUserToken(
                context.activity.from.id,
                this.googleconnectionName,
                context.activity.channelId,
                magicCode
            );
        
            if (!tokenResponse || !tokenResponse.token) {
                // There is no token, so the user has not signed in yet.
                // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                is_fb_signed_in = false;
                is_google_signed_in = true;
                const { signInLink } = await userTokenClient.getSignInResource(
                    this.googleconnectionName,
                    context.activity
                );

                return {
                    composeExtension: {
                        type: 'auth',
                        suggestedActions: {
                            actions: [
                                {
                                    type: 'openUrl',
                                    value: signInLink,
                                    title: 'Google OAuth'
                                },
                            ],
                        },
                    },
                };
            }

            var googleProfileDetails = await Data.getGoogleUserData(tokenResponse.token);
            googleProfile["is_google_signed_in"]= true;
            googleProfile["name"] = googleProfileDetails.names[0].displayName;
            googleProfile["image"] = googleProfileDetails.photos[0].url;
            googleProfile["email"] = googleProfileDetails.emailAddresses[0].value;
            userData['google_id'] = googleProfileDetails.emailAddresses[0].value;
            userData['google_token'] = tokenResponse.token;
            userData['is_google_signed_in'] = true;
            currentData[updateindex] = userData;
            userDetails["userDetails"] = currentData;
            if(userData.is_fb_signed_in){
                var facebookProfileDetail = await Data.getFacebookUserData(userData.facebook_token);
                facebookProfile["is_fb_signed_in"]= true;
                facebookProfile["name"]= facebookProfileDetail.name;
                facebookProfile["image"]= facebookProfileDetail.picture.data.url;
            }
            else{
                facebookProfile["is_fb_signed_in"]= false
            }
            card = CardHelper.getMEResponseCard(ssoData.myDetails, ssoData.photo,facebookProfile,googleProfile);
            return {
                task: {
                    type: 'continue',
                    value: {
                        card: card,
                        heigth: 250,
                        width: 400,
                        title: 'Show Profile Card'
                    },
                },
            };
        }
        if(state == 'dicconnectFromFacebook'){
            is_fb_signed_in = false;
            is_google_signed_in = false;
            userData['facebook_id'] = null;
            userData['facebook_token'] = null;
            userData['is_fb_signed_in'] = false;
            currentData[updateindex] = userData;
            userDetails["userDetails"] = currentData;
            facebookProfile["is_fb_signed_in"]= false;
            if(userData.is_google_signed_in){
                var googleProfileDetails = await Data.getGoogleUserData(userData.google_token);
                googleProfile["is_google_signed_in"]= true;
                googleProfile["name"] = googleProfileDetails.names[0].displayName;
                googleProfile["image"] = googleProfileDetails.photos[0].url;
                googleProfile["email"] = googleProfileDetails.emailAddresses[0].value;
            }
            else{
                googleProfile["is_google_signed_in"]= false
            }
            card = CardHelper.getMEResponseCard(ssoData.myDetails, ssoData.photo,facebookProfile,googleProfile);
            return {
                task: {
                    type: 'continue',
                    value: {
                        card: card,
                        heigth: 250,
                        width: 400,
                        title: 'Show Profile Card'
                    },
                },
            };
        }
        if(state == 'disConnectFromGoogle'){
            is_fb_signed_in = false;
            is_google_signed_in = false;
            userData['google_id'] = null;
            userData['google_token'] = null;
            userData['is_google_signed_in'] = false;
            currentData[updateindex] = userData;
            userDetails["userDetails"] = currentData;
            googleProfile["is_google_signed_in"]= false
            if(userData.is_fb_signed_in){
                var facebookProfileDetail = await Data.getFacebookUserData(userData.facebook_token);
                facebookProfile["is_fb_signed_in"]= true
                facebookProfile["name"]= facebookProfileDetail.name;
                facebookProfile["image"]= facebookProfileDetail.picture.data.url;
            }
            else{
                facebookProfile["is_fb_signed_in"]= false
            }
            card = CardHelper.getMEResponseCard(ssoData.myDetails, ssoData.photo,facebookProfile,googleProfile);
            return {
                task: {
                    type: 'continue',
                    value: {
                        card: card,
                        heigth: 250,
                        width: 400,
                        title: 'Show Profile Card'
                    },
                },
            };
        }
    }

    async handleTeamsMessagingExtensionFetchTask(context, action) {
        const userTokenClient = context.turnState.get(context.adapter.UserTokenClientKey);
        const magicCode =action.state && Number.isInteger(Number(action.state))? action.state: '';

        const tokenResponse = await userTokenClient.getUserToken(
            context.activity.from.id,
            this.connectionName,
            context.activity.channelId,
            magicCode
        );
                
        if (!tokenResponse || !tokenResponse.token) {
            // There is no token, so the user has not signed in yet.
            // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions

            const { signInLink } = await userTokenClient.getSignInResource(
                this.connectionName,
                context.activity
            );

            return {
                composeExtension: {
                    type: 'auth',
                    suggestedActions: {
                        actions: [
                            {
                                type: 'openUrl',
                                value: signInLink,
                                title: 'Bot Service OAuth'
                            },
                        ],
                    },
                },
            };
        }
        else{
            var currentData = userDetails["userDetails"];
            var ssoData = await Data.getAADUserData(tokenResponse.token);
            var facbookProfile = {};
            var googleProfile= {};
            var card;
            if (currentData == undefined) {
                const userDetailsList = new Array();
                userDetailsList.push({ "aad_id": context.activity.from.aadObjectId, "is_aad_signed_in": true, "aad_token": tokenResponse.token });
                currentData = userDetailsList;
                userDetails["userDetails"] = currentData;
                facbookProfile["is_fb_signed_in"]= false;
                googleProfile["is_google_signed_in"]= false;
                card = CardHelper.getMEResponseCard(ssoData.myDetails, ssoData.photo,facbookProfile,googleProfile);
                return {
                    task: {
                        type: 'continue',
                        value: {
                            card: card,
                            heigth: 250,
                            width: 400,
                            title: 'Show Profile Card'
                        },
                    },
                };
            }
            else if (!currentData.find((user) => {
                if (user.aad_id == context.activity.from.aadObjectId) {
                  return true;
                }
              })) {
                const userDetailsList = currentData;
                userDetailsList.push({ "aad_id": context.activity.from.aadObjectId, "is_aad_signed_in": true, "aad_token": tokenResponse.token });
                currentData = userDetailsList;
                userDetails["userDetails"] = currentData;
                facbookProfile["is_fb_signed_in"]= false;
                googleProfile["is_google_signed_in"]= false;
                card = CardHelper.getMEResponseCard(ssoData.myDetails, ssoData.photo,facbookProfile,googleProfile);
                return {
                    task: {
                        type: 'continue',
                        value: {
                            card: card,
                            heigth: 250,
                            width: 400,
                            title: 'Show Profile Card'
                        },
                    },
                };
              }
            else{
                var userData;
                let updateindex;
                currentData.find((user, index) => {
                if (user.aad_id == context.activity.from.aadObjectId) {
                    userData = user;
                    updateindex = index;
                }
                })
                userData["aad_token"] = tokenResponse.token
                currentData[updateindex] = userData;
                userDetails["userDetails"] = currentData;

                if (userData.is_fb_signed_in){
                    var facbookProfileDetail = await Data.getFacebookUserData(userData.facebook_token);
                    facbookProfile["is_fb_signed_in"]= true;
                    facbookProfile["name"]= facbookProfileDetail.name;
                    facbookProfile["image"]= facbookProfileDetail.picture.data.url;
                }
                else{
                    facbookProfile["is_fb_signed_in"]= false;
                }
                if(userData.is_google_signed_in){
                   var googleProfileDetails = await Data.getGoogleUserData(userData.google_token);
                    googleProfile["is_google_signed_in"]= true;
                    googleProfile["name"] = googleProfileDetails.names[0].displayName;
                    googleProfile["image"] = googleProfileDetails.photos[0].url;
                    googleProfile["email"] = googleProfileDetails.emailAddresses[0].value;
                }
                else{
                    googleProfile["is_google_signed_in"]= false;
                }
                card = CardHelper.getMEResponseCard(ssoData.myDetails, ssoData.photo,facbookProfile,googleProfile);
                return {
                    task: {
                        type: 'continue',
                        value: {
                            card: card,
                            heigth: 250,
                            width: 400,
                            title: 'Show Profile Card'
                        },
                    },
                };
            }
        }
    }

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

    getAdaptiveCardUserDetails = () => ({
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        body: [
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: "User profile details are"
            },
            {
                type: "Image",
                size: "Medium",
                url: "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg"
            },
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                wrap: true,
                text: "Hello! Test user"
            },
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: "Job title: Data scientist"
            },
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: 'Email: testaccount@test123.onmicrosoft.com'
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    });
}

module.exports.TeamsBot = TeamsBot;