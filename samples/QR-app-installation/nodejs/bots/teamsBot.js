// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogBot } = require('./dialogBot');
const { tokenExchangeOperationName } = require('botbuilder');
const { SsoOAuthHelpler } = require('../SsoOAuthHelpler');

class TeamsBot extends DialogBot {
    /**
    *
    * @param {ConversationState} conversationState
    * @param {UserState} userState
    * @param {Dialog} dialog
    */
    constructor(conversationState, userState, dialog) {
        super(conversationState, userState, dialog);
        this._ssoOAuthHelper = new SsoOAuthHelpler(process.env.connectionName, conversationState);

        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let member = 0; member < membersAdded.length; member++) {
                if (membersAdded[member].id !== context.activity.recipient.id) {
                    await context.sendActivity('Hello and welcome! With this sample your bot can generate QR code for App and install the app in team by scanning QR code. Please add this bot to team and type hey.');
                }
            }

            await next();
        });
    }

    async onTokenResponseEvent(context) {
        console.log('Running dialog with Token Response Event Activity.');

        // Run the Dialog with the new Token Response Event Activity.
        await this.dialog.run(context, this.dialogState);
    }

    async onSignInInvoke(context) {
        if (context.activity && context.activity.name === tokenExchangeOperationName) {
            // The Token Exchange Helper will attempt the exchange, and if successful, it will cache the result
            // in TurnState.  This is then read by SsoOAuthPrompt, and processed accordingly.
            if (!await this._ssoOAuthHelper.shouldProcessTokenExchange(context)) {
                // If the token is not exchangeable, do not process this activity further.
                // (The Token Exchange Helper will send the appropriate response if the token is not exchangeable)
                return;
            }
        }
        await this.dialog.run(context, this.dialogState);
    }

    async handleTeamsSigninVerifyState(context, query) {
        console.log('Running dialog with signin/verifystate from an Invoke Activity.');
        await this.dialog.run(context, this.dialogState);
    }
}

module.exports.TeamsBot = TeamsBot;