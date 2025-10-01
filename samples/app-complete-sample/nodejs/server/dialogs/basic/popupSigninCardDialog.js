// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory, ActionTypes } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const POPUPSIGNINCARD = 'PopupSignInCard';

/**
 * PopupSigninCardDialog class extends ComponentDialog to handle popup sign-in card interactions.
 */
class PopupSigninCardDialog extends ComponentDialog {
    /**
     * Constructor for the PopupSigninCardDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(POPUPSIGNINCARD, [
            this.beginPopupSigninCardDialog.bind(this),
        ]));
    }

    /**
     * Begins the popup sign-in card dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginPopupSigninCardDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "PopupSignInDialog";
        const reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }

        const buttons = [
            { type: ActionTypes.Signin, title: 'Sign In', value: `${process.env.BaseUri}/tab/tabConfig/popUpSignin.html?height=400&width=400` },
        ];

        const card = CardFactory.heroCard('Please click below for Popup Sign-In experience', undefined, buttons);

        reply.attachments = [card];
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }
}

exports.PopupSigninCardDialog = PopupSigninCardDialog;