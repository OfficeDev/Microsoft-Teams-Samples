// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogSet, DialogTurnStatus, WaterfallDialog,TextPrompt } = require('botbuilder-dialogs');
const { CardFactory,MessageFactory,InputHints,ActionTypes} = require('botbuilder');
const { LogoutDialog } = require('./logoutDialog');
const MAIN_DIALOG = 'MainDialog';
const MAIN_WATERFALL_DIALOG = 'MainWaterfallDialog';
const OAUTH_PROMPT = 'OAuthPrompt';
const TEXT_PROMPT = 'textPrompt';
const { polyfills } = require('isomorphic-fetch');
const { SsoOAuthPrompt } = require('./ssoOAuthPrompt');
const { SimpleGraphClient } = require('../simpleGraphClient');
const { SimpleFacebookAuthDialog } = require('./simpleFacebookAuthDialog');
const FACEBOOKAUTH = 'FacebookAuth';
var  token;

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

        this.addDialog(new TextPrompt(TEXT_PROMPT))
        this.addDialog(new SimpleFacebookAuthDialog(FACEBOOKAUTH));

        this.addDialog(new WaterfallDialog(MAIN_WATERFALL_DIALOG, [
            this.promptStep.bind(this),
            this.loginStep.bind(this),
            this.userInfoStep.bind(this)
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
            if (stepContext.context._activity.text.trim() == "sso") {
                return await stepContext.beginDialog(OAUTH_PROMPT);
            }
            else if (stepContext.context._activity.text.trim() == "usingcredentials") {
                await stepContext.context.sendActivity({ attachments: [this.getAdaptiveCardUserLogin()] });
                return await stepContext.endDialog();
            }
            else if (stepContext.context._activity.text.trim() == "facebooklogin") {
                await stepContext.beginDialog(FACEBOOKAUTH);
                return await stepContext.endDialog();
            }
            else{
                const buttons = [
                    { type: ActionTypes.ImBack, title: 'AAD SSO authentication', value: 'sso' },
                    { type: ActionTypes.ImBack, title: 'Facebook login (OAuth 2)', value: 'facebooklogin' },
                    { type: ActionTypes.ImBack, title: 'User Id/password login', value: 'usingcredentials' }]
                const card = CardFactory.heroCard('Login options', undefined,
                    buttons,{"text":"Select a login option"});
                await stepContext.context.sendActivity({ attachments: [card] });
                return await stepContext.endDialog();
            }
        } catch (err) {
            console.error(err);
        }
    }

    async loginStep(stepContext) {
        // Get the token from the previous step. Note that we could also have gotten the
        // token directly from the prompt itself. There is an example of this in the next method.
        if (stepContext.context._activity.text.trim() == "sso") {
            const tokenResponse = stepContext.result;

            if (!tokenResponse || !tokenResponse.token) {
                await stepContext.context.sendActivity('Login was not successful please try again.');
            }
            else {
                token = tokenResponse.token;
                const messageText = 'What is your favorite color?';
                const msg = MessageFactory.text(messageText, messageText, InputHints.ExpectingInput);
                await stepContext.context.sendActivity('Login successful.');
                return await stepContext.prompt(TEXT_PROMPT, { prompt: msg });
            }
            await stepContext.context.sendActivity("Please type 'sso' to begin authentication");

            return await stepContext.endDialog();
        }
    }

    async userInfoStep(stepContext) {
        const color = stepContext.result
        const client = new SimpleGraphClient(token);
        const myDetails = await client.getMeAsync();
        var imageString = "";
        var img2 = "";

        if (myDetails != null) {
            var userImage = await client.getUserPhoto();
            await userImage.arrayBuffer().then(result => {
                console.log(userImage.type);
                imageString = Buffer.from(result).toString('base64');
                img2 = "data:image/png;base64," + imageString;
            }).catch(error => { console.log(error) });

            const userCard = CardFactory.adaptiveCard(this.getAdaptiveCardUserDetails(myDetails, img2,color));
            await stepContext.context.sendActivity({ attachments: [userCard] });
        }
        return await stepContext.endDialog();
    }
    getAdaptiveCardUserLogin() {
        return CardFactory.heroCard(
            'Signin card',
            undefined,
            CardFactory.actions([
                {
                    type: 'signin',
                    title: 'Get started',
                    value: this.baseUrl+'/popUpSignin?from=bot&height=535&width=600'
                }
            ])
        );
    }

    getAdaptiveCardUserDetails = (myDetails, userImage,color) => ({
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
                url: userImage
            },
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                wrap: true,
                text: `Hello! ${myDetails.displayName}`
            },
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: `Job title: ${myDetails.jobDetails ? myDetails.jobDetails : "Unknown"}`
            },
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: `Email: ${myDetails.userPrincipalName}`
            },
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: `Favorite color: ${color}`
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    });
}

module.exports.MainDialog = MainDialog;