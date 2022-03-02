// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogBot } = require('./dialogBot');
const { tokenExchangeOperationName } = require('botbuilder');
const { SsoOAuthHelpler } = require('../SsoOAuthHelpler');
const { CardFactory,ActionTypes } = require('botbuilder');
const Data = require('../helper/dataHelper');
const CardHelper = require('../cards/cardHelper');
const userDetailsMEAction = {};
const userDetailsMESearch = {};

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
                }
            }

            await next();
        });
    }

    async onSignInInvoke(context){
        await this.dialog.run(context, this.dialogState);
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
        var currentData = userDetailsMEAction["userDetails"];
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
            userDetailsMEAction["userDetails"] = currentData;
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
            userDetailsMEAction["userDetails"] = currentData;
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
            userDetailsMEAction["userDetails"] = currentData;
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
            userDetailsMEAction["userDetails"] = currentData;
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
            var currentData = userDetailsMEAction["userDetails"];
            var ssoData = await Data.getAADUserData(tokenResponse.token);
            var facbookProfile = {};
            var googleProfile= {};
            var card;
            if (currentData == undefined) {
                const userDetailsList = new Array();
                userDetailsList.push({ "aad_id": context.activity.from.aadObjectId, "is_aad_signed_in": true, "aad_token": tokenResponse.token });
                currentData = userDetailsList;
                userDetailsMEAction["userDetails"] = currentData;
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
                userDetailsMEAction["userDetails"] = currentData;
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
                userDetailsMEAction["userDetails"] = currentData;

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

    async handleTeamsMessagingExtensionQuery(context, query) {
        const userTokenClient = context.turnState.get(context.adapter.UserTokenClientKey);
        const magicCode =
            context.state && Number.isInteger(Number(context.state))
                ? context.state
                : '';

        const tokenResponse = await userTokenClient.getUserToken(
            context.activity.from.id,
            this.connectionName,
            context.activity.channelId,
            magicCode
        );
        if (!tokenResponse || !tokenResponse.token) {
            this.isSignedIn = true;
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
                            }
                        ]
                    }
                }
            };
        }
        else{
            var currentData = userDetailsMESearch["userDetails"];
            var ssoData = await Data.getAADUserData(tokenResponse.token);
            if (currentData == undefined) {
                const userDetailsList = new Array();
                userDetailsList.push({ "aad_id": context.activity.from.aadObjectId, "is_aad_signed_in": true, "aad_token": tokenResponse.token,"is_fb_signed_in":false,"is_google_signed_in":false });
                currentData = userDetailsList;
                userDetailsMESearch["userDetails"] = currentData;
                return {
                    composeExtension: {
                        type: 'config',
                        suggestedActions: {
                            actions: [
                                {
                                    type: 'openUrl',
                                    value: `${this.baseUrl}/config?is_fb_signed_in=false&is_google_signed_in=false`,
                                    title: 'Connect'
                                }
                            ]
                        }
                    }
                };
            }
            else if(!currentData.find((user) => {
                if (user.aad_id == context.activity.from.aadObjectId) {
                  return true;
                }
              })){
                const userDetailsList = currentData;
                userDetailsList.push({ "aad_id": context.activity.from.aadObjectId, "is_aad_signed_in": true, "aad_token": tokenResponse.token,"is_fb_signed_in":false,"is_google_signed_in":false });
                currentData = userDetailsList;
                userDetailsMESearch["userDetails"] = currentData;
                return {
                    composeExtension: {
                        type: 'config',
                        suggestedActions: {
                            actions: [
                                {
                                    type: 'openUrl',
                                    value: `${this.baseUrl}/config?is_fb_signed_in=false&is_google_signed_in=false`,
                                    title: 'Connect'
                                }
                            ]
                        }
                    }
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
                userDetailsMESearch["userDetails"] = currentData;

                if((!userData.is_fb_signed_in && query.state == undefined) ||(!userData.is_google_signed_in && query.state == undefined)){
                    return {
                        composeExtension: {
                            type: 'config',
                            suggestedActions: {
                                actions: [
                                    {
                                        type: 'openUrl',
                                        value: `${this.baseUrl}/config?is_fb_signed_in=${userData.is_fb_signed_in}&is_google_signed_in=${userData.is_google_signed_in}`,
                                        title: 'Connect'
                                    }
                                ]
                            }
                        }
                    };
                }
                else if(query.state == "ConnectWithFacebook" || is_fb_signed_in){
                    const userTokenClient = context.turnState.get(context.adapter.UserTokenClientKey);
                    const magicCode =
                    context.state && Number.isInteger(Number(context.state))
                    ? context.state
                    : '';

                    const tokenResponse = await userTokenClient.getUserToken(
                        context.activity.from.id,
                        this.fbconnectionName,
                        context.activity.channelId,
                        magicCode
                    );
                    if (!tokenResponse || !tokenResponse.token) {
                        is_fb_signed_in = true;
                        is_google_signed_in = false;
                        // There is no token, so the user has not signed in yet.
                        // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
        
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
                                            title: 'Facebook login'
                                        },
                                    ],
                                },
                            },
                        };
                    }
                    else{
                        is_fb_signed_in = false;
                        var facebookProfileDetail = await Data.getFacebookUserData(tokenResponse.token);
                        userData['facebook_id'] = facebookProfileDetail.id;
                        userData['facebook_token'] = tokenResponse.token;
                        userData['is_fb_signed_in'] = true;
                        currentData[updateindex] = userData;
                        userDetailsMESearch["userDetails"] = currentData;
                        const ssoAttachment = CardFactory.thumbnailCard(
                            'User Profile card',
                            ssoData.myDetails.displayName,
                            CardFactory.images([
                                ssoData.photo
                            ])
                        );
                        const fbattachment = CardFactory.thumbnailCard(
                            'Facebook profile card',
                            facebookProfileDetail.name,
                            CardFactory.images([
                                facebookProfileDetail.picture.data.url
                            ])
                        );
                        if(userData.is_google_signed_in){
                            var googleProfileDetails = await Data.getGoogleUserData(userData.google_token);
                            const googleAttachment = CardFactory.thumbnailCard(
                                'Google profile card',
                                googleProfileDetails.names[0].displayName,
                                CardFactory.images([
                                    googleProfileDetails.photos[0].url
                                ])
                            );
                            return {
                                composeExtension: {
                                    attachmentLayout: 'list',
                                    type: 'result',
                                    attachments: [ssoAttachment,fbattachment,googleAttachment]
                                }
                            }
                        }
                        else{
                            return {
                                composeExtension: {
                                    attachmentLayout: 'list',
                                    type: 'result',
                                    attachments: [ssoAttachment,fbattachment]
                                }
                            }; 
                        }
                    }
                }
                else if(query.state == "ConnectWithGoogle" || is_google_signed_in){
                    const userTokenClient = context.turnState.get(context.adapter.UserTokenClientKey);
                    const magicCode =
                    context.state && Number.isInteger(Number(context.state))
                    ? context.state
                    : '';

                    const tokenResponse = await userTokenClient.getUserToken(
                        context.activity.from.id,
                        this.googleconnectionName,
                        context.activity.channelId,
                        magicCode
                    );
                    if (!tokenResponse || !tokenResponse.token) {
                        is_google_signed_in = true;
                        is_fb_signed_in = false;
                        // There is no token, so the user has not signed in yet.
                        // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
        
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
                                            title: 'Google login'
                                        },
                                    ],
                                },
                            },
                        };
                    }
                    else if(query.state == "DisconnectFromGoogle"){
                        userData['google_id'] = null;
                        userData['google_token'] = null;
                        userData['is_google_signed_in'] = false;
                        currentData[updateindex] = userData;
                        userDetailsMESearch["userDetails"] = currentData;
                        return {
                            composeExtension: {
                                type: 'config',
                                suggestedActions: {
                                    actions: [
                                        {
                                            type: 'openUrl',
                                            value: `${this.baseUrl}/config?is_fb_signed_in=${userData.is_fb_signed_in}&is_google_signed_in=${userData.is_google_signed_in}`,
                                            title: 'Connect'
                                        }
                                    ]
                                }
                            }
                        };
                    }
                    else if(query.state == "DisconnectFromFacebook"){
                        userData['facebook_id'] = null;
                        userData['facebook_token'] = null;
                        userData['is_fb_signed_in'] = false;
                        currentData[updateindex] = userData;
                        userDetailsMESearch["userDetails"] = currentData;
                        return {
                            composeExtension: {
                                type: 'config',
                                suggestedActions: {
                                    actions: [
                                        {
                                            type: 'openUrl',
                                            value: `${this.baseUrl}/config?is_fb_signed_in=${userData.is_fb_signed_in}&is_google_signed_in=${userData.is_google_signed_in}`,
                                            title: 'Connect'
                                        }
                                    ]
                                }
                            }
                        };
                    }
                    else{
                        is_google_signed_in = false;
                        var googleProfileDetails = await Data.getGoogleUserData(tokenResponse.token);
                        userData['google_id'] = googleProfileDetails.emailAddresses[0].value;;
                        userData['google_token'] = tokenResponse.token;
                        userData['is_google_signed_in'] = true;
                        currentData[updateindex] = userData;
                        userDetailsMESearch["userDetails"] = currentData;
                        const ssoAttachment = CardFactory.thumbnailCard(
                            'User Profile card',
                            ssoData.myDetails.displayName,
                            CardFactory.images([
                                ssoData.photo
                            ])
                        );
                        const googleAttachment = CardFactory.thumbnailCard(
                            'Google profile card',
                            googleProfileDetails.names[0].displayName,
                            CardFactory.images([
                                googleProfileDetails.photos[0].url
                            ])
                        );
                        if(userData.is_fb_signed_in){
                            var facebookProfileDetail = await Data.getFacebookUserData(userData.facebook_token);
                            const fbattachment = CardFactory.thumbnailCard(
                                'Facebook profile card',
                                facebookProfileDetail.name,
                                CardFactory.images([
                                    facebookProfileDetail.picture.data.url
                                ])
                            );
                            return {
                                composeExtension: {
                                    attachmentLayout: 'list',
                                    type: 'result',
                                    attachments: [ssoAttachment,fbattachment,googleAttachment]
                                }
                            }
                        }
                        else{
                            return {
                                composeExtension: {
                                    attachmentLayout: 'list',
                                    type: 'result',
                                    attachments: [ssoAttachment,googleAttachment]
                                }
                            };

                        }
                    }
                }
                else{
                    const ssoAttachment = CardFactory.thumbnailCard(
                        'User Profile card',
                        ssoData.myDetails.displayName,
                        CardFactory.images([
                            ssoData.photo
                        ])
                    );
                    var facebookProfileDetail = await Data.getFacebookUserData(userData.facebook_token);
                            const fbattachment = CardFactory.thumbnailCard(
                                'Facebook profile card',
                                facebookProfileDetail.name,
                                CardFactory.images([
                                    facebookProfileDetail.picture.data.url
                                ])
                            );
                    var googleProfileDetails = await Data.getGoogleUserData(userData.google_token);
                    const googleAttachment = CardFactory.thumbnailCard(
                        'Google profile card',
                        googleProfileDetails.names[0].displayName,
                        CardFactory.images([
                            googleProfileDetails.photos[0].url
                        ])
                    ); 
                    return {
                        composeExtension: {
                            attachmentLayout: 'list',
                            type: 'result',
                            attachments: [ssoAttachment,fbattachment,googleAttachment]
                        }
                    }           
                }
            }
        }
    }

    async handleTeamsMessagingExtensionConfigurationQuerySettingUrl(
        context,
        query
    ){
        var currentData = userDetailsMESearch["userDetails"];
        if (currentData == undefined){
            return null;
        }
        else if(!currentData.find((user) => {
            if (user.aad_id == context.activity.from.aadObjectId) {
              return true;
            }
          })){
            return null;
          }
        else{
            var userData;
            currentData.find((user) => {
                if (user.aad_id == context.activity.from.aadObjectId) {
                    userData = user;
                }
            })
            return {
                composeExtension: {
                    type: 'config',
                    suggestedActions: {
                        actions: [
                            {
                                type: ActionTypes.OpenUrl,
                                value: `${this.baseUrl}/config?is_fb_signed_in=${userData.is_fb_signed_in}&is_google_signed_in=${userData.is_google_signed_in}`
                            },
                        ],
                    },
                },
            };
        }
    }

    async handleTeamsMessagingExtensionConfigurationSetting(context, settings) {
        var currentData = userDetailsMESearch["userDetails"];
        var userData;
        let updateindex;
        currentData.find((user, index) => {
          if (user.aad_id == context.activity.from.aadObjectId) {
            userData = user;
            updateindex = index;
          }
        })
        // When the user submits the settings page, this event is fired.
        if (settings.state =="ConnectWithFacebook") {
            is_fb_signed_in = true;
        }
        else if(settings.state =="ConnectWithGoogle") {
            is_google_signed_in = true;
        }
        else if(settings.state =="DisconnectFromFacebook"){
            is_fb_signed_in = false;
            userData['facebook_id'] = null;
            userData['facebook_token'] = null;
            userData['is_fb_signed_in'] = false;
            currentData[updateindex] = userData;
            userDetailsMESearch["userDetails"] = currentData;
        }
        else if(settings.state =="DisconnectFromGoogle"){
            is_google_signed_in = false;
            userData['google_id'] = null;
            userData['google_token'] = null;
            userData['is_google_signed_in'] = false;
            currentData[updateindex] = userData;
            userDetailsMESearch["userDetails"] = currentData;
        }
    }
}

module.exports.TeamsBot = TeamsBot;