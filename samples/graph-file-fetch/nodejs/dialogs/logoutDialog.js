// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityTypes } = require('botbuilder');
const { ComponentDialog } = require('botbuilder-dialogs');

class LogoutDialog extends ComponentDialog {
    constructor(id, connectionName) {
        super(id);
        this.connectionName = connectionName;
    }

    async onBeginDialog(innerDc, options) {
        const result = await this.interrupt(innerDc);
        if (result) {
            return result;
        }

        return await super.onBeginDialog(innerDc, options);
    }

    async onContinueDialog(innerDc) {
        const result = await this.interrupt(innerDc);
        if (result) {
            return result;
        }

        return await super.onContinueDialog(innerDc);
    }

    async interrupt(innerDc) {
        if (innerDc.context.activity.type === ActivityTypes.Message) {
            const text = innerDc.context.activity.text.toLowerCase();
            // Remove the line break
            if (text.replace(/\r?\n|\r/g, '') === 'logout') {
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
