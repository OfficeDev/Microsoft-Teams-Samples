// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogSet, DialogTurnStatus, WaterfallDialog } = require('botbuilder-dialogs');
const { LogoutDialog } = require('./logoutDialog');
const MAIN_DIALOG = 'MainDialog';
const MAIN_WATERFALL_DIALOG = 'MainWaterfallDialog';
const OAUTH_PROMPT = 'OAuthPrompt';
const { SsoOAuthPrompt } = require('./ssoOAuthPrompt');
const { polyfills } = require('isomorphic-fetch');
const adaptiveCards = require('../models/adaptiveCard');
const { CardFactory } = require('botbuilder');
const Token_State_Property = 'TokenData';

class MainDialog extends LogoutDialog {
    constructor(conversationState) {
        super(MAIN_DIALOG, process.env.connectionName);
        this.conversationDataAccessor = conversationState.createProperty(Token_State_Property);

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

            if(stepContext.context._activity.conversation.conversationType!="personal")
            {
                var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
                currentState.token = tokenResponse.token;
                await stepContext.context.sendActivity({ attachments: [CardFactory.adaptiveCard(adaptiveCards.getAdaptiveCardUserDetails())] });
            }  
        }

        return await stepContext.endDialog();
    }
}

module.exports.MainDialog = MainDialog;