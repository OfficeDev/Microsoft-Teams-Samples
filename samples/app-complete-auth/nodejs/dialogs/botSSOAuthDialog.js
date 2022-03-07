// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog,TextPrompt} = require('botbuilder-dialogs');
const { CardFactory,MessageFactory,InputHints} = require('botbuilder');

const { LogoutDialog } = require('./logoutDialog');
const { SsoOAuthPrompt } = require('./ssoOAuthPrompt');
const { SimpleGraphClient } = require('../simpleGraphClient');
const SSOAUTH = 'SsoAuth';
const OAUTH_PROMPT = 'OAuthPrompt';
const TEXT_PROMPT = 'textPrompt';
var  token;

class BotSSOAuthDialog extends LogoutDialog {
    constructor(id) {
        super(id, process.env.connectionName);

        this.addDialog(new SsoOAuthPrompt(OAUTH_PROMPT, {
            connectionName: process.env.connectionName,
            text: 'Please Sign In',
            title: 'Sign In',
            timeout: 300000
        }));
        this.addDialog(new TextPrompt(TEXT_PROMPT));
        this.addDialog(new WaterfallDialog(SSOAUTH, [
            this.promptStep.bind(this),
            this.loginStep.bind(this),
            this.userInfoStep.bind(this)
        ]));
        this.initialDialogId = SSOAUTH;
    }

    async promptStep(stepContext) {
        return await stepContext.beginDialog(OAUTH_PROMPT);
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
                const messageText = 'What is your user name?';
                const msg = MessageFactory.text(messageText, messageText, InputHints.ExpectingInput);
                await stepContext.context.sendActivity('Login successful.');
                return await stepContext.prompt(TEXT_PROMPT, { prompt: msg });
            }
            await stepContext.context.sendActivity("Please type 'sso' to begin authentication");

            return await stepContext.endDialog();
        }
    }

    async userInfoStep(stepContext) {
        const userName = stepContext.result
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

            const userCard = CardFactory.adaptiveCard(this.getAdaptiveCardUserDetails(myDetails, img2,userName));
            await stepContext.context.sendActivity({ attachments: [userCard] });
        }
        return await stepContext.endDialog();
    }

    getAdaptiveCardUserDetails = (myDetails, userImage,userName) => ({
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
                text: `User name : ${userName}`
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    });
}

exports.BotSSOAuthDialog = BotSSOAuthDialog;