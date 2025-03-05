// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogSet, DialogTurnStatus, WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const { CardFactory, ActionTypes } = require('botbuilder');

const MAIN_DIALOG = 'MainDialog';
const MAIN_WATERFALL_DIALOG = 'MainWaterfallDialog';

const { SimpleFacebookAuthDialog } = require('./simpleFacebookAuthDialog');
const { UserNamePasswordDialog } = require('./userNamePasswordDialog');
const { BotSSOAuthDialog } = require('./botSSOAuthDialog');

const FACEBOOKAUTH = 'FacebookAuth';
const USERNAMEPASSWORD = 'UserNamePassword';
const SSOAUTH = 'SsoAuth';

/**
 * MainDialog class handles the main dialog process.
 */
class MainDialog extends ComponentDialog {
    constructor() {
        super(MAIN_DIALOG, process.env.connectionName);
        this.baseUrl = process.env.ApplicationBaseUrl;

        this.addDialog(new SimpleFacebookAuthDialog(FACEBOOKAUTH));
        this.addDialog(new BotSSOAuthDialog(SSOAUTH));
        this.addDialog(new UserNamePasswordDialog(USERNAMEPASSWORD));
        this.addDialog(new WaterfallDialog(MAIN_WATERFALL_DIALOG, [
            this.promptStep.bind(this)
        ]));

        this.initialDialogId = MAIN_WATERFALL_DIALOG;
    }

    /**
     * The run method handles the incoming activity (in the form of a DialogContext) and passes it through the dialog system.
     * If no dialog is active, it will start the default dialog.
     * @param {Object} context - The context of the current turn of conversation.
     * @param {Object} accessor - The state accessor for the dialog.
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

    /**
     * The promptStep method handles the initial prompt step of the dialog.
     * @param {Object} stepContext - The context of the current step.
     */
    async promptStep(stepContext) {
        try {
            const text = stepContext.context._activity.text;
            if (!text || text.trim() === "usingcredentials") {
                return await stepContext.beginDialog(USERNAMEPASSWORD);
            } else if (text.trim() === "sso") {
                return await stepContext.beginDialog(SSOAUTH);
            } else if (text.trim() === "facebooklogin") {
                return await stepContext.beginDialog(FACEBOOKAUTH);
            } else {
                const buttons = [
                    { type: ActionTypes.ImBack, title: 'AAD SSO authentication', value: 'sso' },
                    { type: ActionTypes.ImBack, title: 'Facebook login (OAuth 2)', value: 'facebooklogin' },
                    { type: ActionTypes.ImBack, title: 'User Id/password login', value: 'usingcredentials' }
                ];
                const card = CardFactory.heroCard('Login options', undefined, buttons, { text: "Select a login option" });
                await stepContext.context.sendActivity({ attachments: [card] });
                return await stepContext.endDialog();
            }
        } catch (err) {
            console.error(err);
        }
    }
}

module.exports.MainDialog = MainDialog;