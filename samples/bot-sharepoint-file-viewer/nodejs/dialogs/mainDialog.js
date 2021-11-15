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
        }
        else {

            if (stepContext.context._activity.text == "viewfile") {
                const client = new SimpleGraphClient(tokenResponse.token);
                const site = await client.getSiteDetails(process.env.SharePointTenantName, process.env.SharePointSiteName);

                if (site != null) {
                    var drive = await client.getDriveDetails(site.id);

                    if (drive != null) {
                        var children = await client.getContentList(site.id, drive.value[0].id);
                        if (children != null) {
                            var fileList = new Array();
                            children.value.map((file) => {
                                if (file.name != "General")
                                    fileList.push(file.name);
                            })

                            var fileUrl = "";
                            var actions = new Array();
                            fileList.map((file) => {
                                var extension = file.split(".")[1];
                                fileUrl = "https://teams.microsoft.com/_#/" + extension + "/viewer/teams/https:~2F~2F" + process.env.SharepointTenantName + "~2Fsites~2F" + process.env.SharepointSiteName + "~2FShared%20Documents~2F" + file
                                actions.push({
                                    type: "Action.OpenUrl",
                                    title: file.split(".")[0],
                                    url: fileUrl
                                });
                            })

                            const userCard = CardFactory.adaptiveCard(this.getAdaptiveCardUserDetails(actions));
                            await stepContext.context.sendActivity({ attachments: [userCard] });
                        }
                    }
                }
                return await stepContext.endDialog();
            }

            if (stepContext.context._activity.text == "uploadfile") {
                tokenData["token"] = tokenResponse.token;
                const userCard = CardFactory.adaptiveCard(this.getAdaptiveCardForFileUpload());
                await stepContext.context.sendActivity({ attachments: [userCard] });

                return await stepContext.endDialog();
            }
        }
        
        await stepContext.context.sendActivity("Please type 'uploadfile' to upload file to SharePoint site or 'viewfile' to get card for file viewer");

        return await stepContext.endDialog();
    }

    getAdaptiveCardUserDetails = (actions) => ({
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        body: [
            {
                type: 'TextBlock',
                text: 'Click on the button to view file in file viewer',
                weight: 'bolder',
                size: 3
            }
        ],
        actions: actions,
        type: 'AdaptiveCard',
        version: '1.4'
    });

    getAdaptiveCardForFileUpload = () => ({
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        body: [
            {
                type: "TextBlock",
                size: "Medium",
                weight: "Bolder",
                text: "Click on button to upload file in sharepoint site"
            },
            {
                type: "ActionSet",
                actions: [
                    {
                        type: "Action.Submit",
                        title: "Upload File",
                        data: {
                            msteams: {
                                type: "task/fetch"
                            },
                            id: "upload"
                        }
                    }
                ]
            }
        ],
        type: "AdaptiveCard",
        version: '1.4'
    });
}

module.exports.MainDialog = MainDialog;
exports.tokenData = tokenData;