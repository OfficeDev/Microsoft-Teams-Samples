// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityTypes } = require('botbuilder');
const { ComponentDialog } = require('botbuilder-dialogs');

/**
 * LogoutDialog class extends ComponentDialog to handle user logout interactions.
 */
class LogoutDialog extends ComponentDialog {
    /**
     * Constructor for the LogoutDialog class.
     * @param {string} id - The dialog ID.
     * @param {string} connectionName - The connection name for the authentication provider.
     */
    constructor(id, connectionName) {
        super(id);
        this.connectionName = connectionName;
    }

    /**
     * Begins the dialog and checks for any interruptions.
     * @param {DialogContext} innerDc - The dialog context.
     * @param {Object} options - The options for the dialog.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async onBeginDialog(innerDc, options) {
        const result = await this.interrupt(innerDc);
        if (result) {
            return result;
        }

        return await super.onBeginDialog(innerDc, options);
    }

    /**
     * Continues the dialog and checks for any interruptions.
     * @param {DialogContext} innerDc - The dialog context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async onContinueDialog(innerDc) {
        const result = await this.interrupt(innerDc);
        if (result) {
            return result;
        }

        return await super.onContinueDialog(innerDc);
    }

    /**
     * Checks for any interruptions, such as a logout command.
     * @param {DialogContext} innerDc - The dialog context.
     * @returns {Promise<DialogTurnResult>} The result of the interruption check.
     */
    async interrupt(innerDc) {
        if (innerDc.context.activity.type === ActivityTypes.Message) {
            const text = innerDc.context.activity.text.toLowerCase();
            if (text.includes('logout')) {
                // The bot adapter encapsulates the authentication processes.
                const botAdapter = innerDc.context.adapter;
                await botAdapter.signOutUser(innerDc.context, this.connectionName);
                await innerDc.context.sendActivity('You have been signed out.');
                return await innerDc.cancelAllDialogs();
            }
        }
    }
}

module.exports.LogoutDialog = LogoutDialog;