// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { BotFrameworkAdapter, ActivityTypes } = require('botbuilder');
const { ComponentDialog } = require('botbuilder-dialogs');

/**
 * LogoutDialog class handles the logout process.
 */
class LogoutDialog extends ComponentDialog {
    /**
     * Creates a new instance of the LogoutDialog class.
     * @param {string} id - The dialog ID.
     * @param {string} connectionName - The connection name for the OAuth provider.
     */
    constructor(id, connectionName) {
        super(id);
        this.connectionName = connectionName;
    }

    /**
     * Called when the dialog is started and pushed onto the dialog stack.
     * @param {Object} innerDc - The dialog context for the current turn of conversation.
     * @param {Object} options - Optional. Initial information to pass to the dialog.
     */
    async onBeginDialog(innerDc, options) {
        const result = await this.interrupt(innerDc);
        if (result) {
            return result;
        }

        return await super.onBeginDialog(innerDc, options);
    }

    /**
     * Called when the dialog is continued, where it is the active dialog and the user replies with a new activity.
     * @param {Object} innerDc - The dialog context for the current turn of conversation.
     */
    async onContinueDialog(innerDc) {
        const result = await this.interrupt(innerDc);
        if (result) {
            return result;
        }

        return await super.onContinueDialog(innerDc);
    }

    /**
     * Checks for the 'logout' command and signs the user out if detected.
     * @param {Object} innerDc - The dialog context for the current turn of conversation.
     */
    async interrupt(innerDc) {
        if (innerDc.context.activity.type === ActivityTypes.Message) {
            const text = innerDc.context.activity.text.trim().toLowerCase();
            if (text === 'logout') {
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