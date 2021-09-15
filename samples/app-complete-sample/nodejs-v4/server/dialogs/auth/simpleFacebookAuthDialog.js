// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ConfirmPrompt, OAuthPrompt, WaterfallDialog } = require('botbuilder-dialogs');

const { LogoutDialog } = require('./logoutDialog');

const CONFIRM_PROMPT = 'ConfirmPrompt';
const FACEBOOKAUTH = 'FacebookAuth';
const OAUTH_PROMPT = 'OAuthPrompt';

class SimpleFacebookAuthDialog extends LogoutDialog {
    constructor(id,conversationDataAccessor) {
        super(id, process.env.ConnectionName);
        this.conversationDataAccessor = conversationDataAccessor;
        this.addDialog(new OAuthPrompt(OAUTH_PROMPT, {
            connectionName: process.env.ConnectionName,
            text: 'Login to facebook',
            title: 'Log In',
            timeout: 300000
        }));
        this.addDialog(new ConfirmPrompt(CONFIRM_PROMPT));
        this.addDialog(new WaterfallDialog(FACEBOOKAUTH, [
            this.promptStep.bind(this),
            this.loginStep.bind(this),
            this.displayTokenPhase1.bind(this),
            this.displayTokenPhase2.bind(this)
        ]));
    }

    async promptStep(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "FacebookAuthDialog";
        return await stepContext.beginDialog(OAUTH_PROMPT);
    }

    async loginStep(stepContext) {
        // Get the token from the previous step. Note that we could also have gotten the
        // token directly from the prompt itself. There is an example of this in the next method.
        const tokenResponse = stepContext.result;
        if (tokenResponse) {
            await stepContext.context.sendActivity('You are now logged in.');
            return await stepContext.prompt(CONFIRM_PROMPT, 'Would you like to view your token?');
        }
        await stepContext.context.sendActivity('Login was not successful please try again.');
        return await stepContext.endDialog();
    }

    async displayTokenPhase1(stepContext) {
        await stepContext.context.sendActivity('Thank you.');

        const result = stepContext.result;
        if (result) {
            // Call the prompt again because we need the token. The reasons for this are:
            // 1. If the user is already logged in we do not need to store the token locally in the bot and worry
            // about refreshing it. We can always just call the prompt again to get the token.
            // 2. We never know how long it will take a user to respond. By the time the
            // user responds the token may have expired. The user would then be prompted to login again.
            //
            // There is no reason to store the token locally in the bot because we can always just call
            // the OAuth prompt to get the token or get a new token if needed.
            return await stepContext.beginDialog(OAUTH_PROMPT);
        }
        return await stepContext.endDialog();
    }

    async displayTokenPhase2(stepContext) {
        const tokenResponse = stepContext.result;
        if (tokenResponse) {
            await stepContext.context.sendActivity(`Here is your token ${ tokenResponse.token }`);
        }
        return await stepContext.endDialog();
    }
}

exports.SimpleFacebookAuthDialog = SimpleFacebookAuthDialog;