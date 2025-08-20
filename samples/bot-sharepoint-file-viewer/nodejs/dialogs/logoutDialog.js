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
                // For CloudAdapter, use UserTokenClient for sign out
                const userTokenClient = innerDc.context.turnState.get(innerDc.context.adapter.UserTokenClientKey);
                if (userTokenClient) {
                    await userTokenClient.signOutUser(
                        innerDc.context.activity.from.id,
                        this.connectionName,
                        innerDc.context.activity.channelId
                    );
                } else {
                    // Fallback to adapter method if available
                    const botAdapter = innerDc.context.adapter;
                    await botAdapter.signOutUser(innerDc.context, this.connectionName);
                }
                await innerDc.context.sendActivity('You have been signed out.');
                return await innerDc.cancelAllDialogs();
            }
        }
    }
}

module.exports.LogoutDialog = LogoutDialog;