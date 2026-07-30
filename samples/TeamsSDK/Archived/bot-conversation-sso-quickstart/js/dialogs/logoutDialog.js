// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityTypes } = require('botbuilder');
const { ComponentDialog } = require('botbuilder-dialogs');

/**
 * LogoutDialog class extends ComponentDialog to handle user logout.
 */
class LogoutDialog extends ComponentDialog {
    /**
     * Creates an instance of LogoutDialog.
     * @param {string} id - The dialog ID.
     * @param {string} connectionName - The connection name for the OAuth provider.
     */
    constructor(id, connectionName) {
        super(id);
        this.connectionName = connectionName;
    }

    /**
     * Called when the dialog is started and pushed onto the dialog stack.
     * @param {DialogContext} innerDc - The dialog context for the current turn of conversation.
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
     * @param {DialogContext} innerDc - The dialog context for the current turn of conversation.
     */
    async onContinueDialog(innerDc) {
        const result = await this.interrupt(innerDc);
        if (result) {
            return result;
        }

        return await super.onContinueDialog(innerDc);
    }

    /**
     * Interrupts the dialog to handle the 'logout' command.
     * @param {DialogContext} innerDc - The dialog context for the current turn of conversation.
     */
    async interrupt(innerDc) {
        if (innerDc.context.activity.type === ActivityTypes.Message) {
            const text = innerDc.context.activity.text.toLowerCase();
            if (text === 'logout') {
                const userTokenClient = innerDc.context.turnState.get(innerDc.context.adapter.UserTokenClientKey);

                const { activity } = innerDc.context;
                await userTokenClient.signOutUser(activity.from.id, this.connectionName, activity.channelId);

                await innerDc.context.sendActivity('You have been signed out.');
                return await innerDc.cancelAllDialogs();
            }
        }
    }
}

module.exports.LogoutDialog = LogoutDialog;