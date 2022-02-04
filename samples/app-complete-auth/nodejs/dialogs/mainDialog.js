// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogSet, DialogTurnStatus, WaterfallDialog } = require('botbuilder-dialogs');
const { CardFactory } = require('botbuilder');
const { LogoutDialog } = require('./logoutDialog');
const MAIN_DIALOG = 'MainDialog';
const MAIN_WATERFALL_DIALOG = 'MainWaterfallDialog';
const OAUTH_PROMPT = 'OAuthPrompt';
const { polyfills } = require('isomorphic-fetch');
const { SsoOAuthPrompt } = require('./ssoOAuthPrompt');
const { SimpleGraphClient } = require('../simpleGraphClient');
const { SimpleFacebookAuthDialog } = require('./simpleFacebookAuthDialog');
const FACEBOOKAUTH = 'FacebookAuth';
const tokenData = {};

class MainDialog extends LogoutDialog {
    constructor() {
        super(MAIN_DIALOG, process.env.connectionName);

        this.addDialog(new SsoOAuthPrompt(OAUTH_PROMPT, {
            connectionName: process.env.connectionName,
            text: 'Please Sign In',
            title: 'Sign In',
            timeout: 300000
        }));

        this.addDialog(new SimpleFacebookAuthDialog(FACEBOOKAUTH));

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
            if (stepContext.context._activity.text.trim() == "sso") {
                return await stepContext.beginDialog(OAUTH_PROMPT);
            }
            else {
                await stepContext.beginDialog(FACEBOOKAUTH);
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

                if (stepContext.context._activity.text.trim() == "sso") {
                    const client = new SimpleGraphClient(tokenResponse.token);
                    const myDetails = await client.getMeAsync();
                    var imageString = "";
                    var img2 = "";

                    if (myDetails != null) {
                        var jobTitle = myDetails.jobTitle ? myDetails.jobDetails : "Unknown";
                        var userImage = await client.getUserPhoto();
                        await userImage.arrayBuffer().then(result => {
                            console.log(userImage.type);
                            imageString = Buffer.from(result).toString('base64');
                            img2 = "data:image/png;base64," + imageString;
                        }).catch(error => { console.log(error) });

                        const userCard = CardFactory.adaptiveCard(this.getAdaptiveCardUserDetails(myDetails, img2));
                        await stepContext.context.sendActivity({ attachments: [userCard] });
                    }
                    return await stepContext.endDialog();
                }
            }

            await stepContext.context.sendActivity("Please type 'sso' to begin authentication");

            return await stepContext.endDialog();
        }
    }

    getAdaptiveCardUserDetails = (myDetails, userImage) => ({
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        body: [
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: "User profile details are"
            },
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                wrap: true,
                text: `Username is ${myDetails.displayName} and email is ${myDetails.userPrincipalName}`
            },
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: `Your job title is ${myDetails.jobDetails ? myDetails.jobDetails : "Unknown"}`
            },
            {
                type: "Image",
                size: "Medium",
                url: userImage
            },
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    });
}

module.exports.MainDialog = MainDialog;
exports.tokenData = tokenData;