// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ConfirmPrompt, DialogSet, DialogTurnStatus, OAuthPrompt, WaterfallDialog } = require('botbuilder-dialogs');
const { LogoutDialog } = require('./logoutDialog');

const CONFIRM_PROMPT = 'ConfirmPrompt';
const MAIN_DIALOG = 'MainDialog';
const MAIN_WATERFALL_DIALOG = 'MainWaterfallDialog';
const OAUTH_PROMPT = 'OAuthPrompt';

/**
 * MainDialog class that extends LogoutDialog to handle the main dialog flow.
 */
class MainDialog extends LogoutDialog {
    /**
     * Creates an instance of MainDialog.
     * @param {string} id - The dialog ID.
     * @param {string} connectionName - The connection name for the OAuth provider.
     */
    constructor() {
        super(MAIN_DIALOG, process.env.connectionName);

        this.addDialog(new OAuthPrompt(OAUTH_PROMPT, {
            connectionName: process.env.connectionName,
            text: 'Please Sign In',
            title: 'Sign In',
            timeout: 300000
        }));
        this.addDialog(new ConfirmPrompt(CONFIRM_PROMPT));
        this.addDialog(new WaterfallDialog(MAIN_WATERFALL_DIALOG, [
            this.promptStep.bind(this),
            this.loginStep.bind(this),
            this.displayTokenPhase1.bind(this),
            this.displayTokenPhase2.bind(this)
        ]));

        this.initialDialogId = MAIN_WATERFALL_DIALOG;
    }

    /**
     * The run method handles the incoming activity (in the form of a DialogContext) and passes it through the dialog system.
     * If no dialog is active, it will start the default dialog.
     * @param {TurnContext} context - The context object for the turn.
     * @param {StatePropertyAccessor} accessor - The state property accessor for the dialog state.
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
     * Prompts the user to sign in.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     */
    async promptStep(stepContext) {
        return await stepContext.beginDialog(OAUTH_PROMPT);
    }

    /**
     * Handles the login step.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     */
    async loginStep(stepContext) {
        const tokenResponse = stepContext.result;
        if (tokenResponse) {
            await stepContext.context.sendActivity('You are now logged in.');
            return await stepContext.prompt(CONFIRM_PROMPT, 'Would you like to view your token?');
        }
        await stepContext.context.sendActivity('Login was not successful, please try again.');
        return await stepContext.endDialog();
    }

    /**
     * Displays the token if the user confirms.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     */
    async displayTokenPhase1(stepContext) {
        await stepContext.context.sendActivity('Thank you.');

        const result = stepContext.result;
        if (result) {
            return await stepContext.beginDialog(OAUTH_PROMPT);
        }
        return await stepContext.endDialog();
    }

    /**
     * Displays the token to the user.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     */
    async displayTokenPhase2(stepContext) {
        const tokenResponse = stepContext.result;
        if (tokenResponse) {
            await stepContext.context.sendActivity(`Here is your token: ${tokenResponse.token}`);
        }
        return await stepContext.endDialog();
    }
}

module.exports.MainDialog = MainDialog;