// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory, ActionTypes } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const AUTHCARD = 'AuthCard';

/**
 * AuthCardDialog class extends ComponentDialog to handle authentication card interactions.
 */
class AuthCardDialog extends ComponentDialog {
    /**
     * Constructor for the AuthCardDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(AUTHCARD, [
            this.beginAuthCardDialog.bind(this),
        ]));
    }

    /**
     * Begins the authentication card dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginAuthCardDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "AuthCardDialog";
        const reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }
        reply.attachments = [this.createAuthSampleCard()];
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }

    /**
     * Creates an authentication sample card.
     * @returns {Attachment} The authentication sample card attachment.
     */
    createAuthSampleCard() {
        const buttons = [
            { type: ActionTypes.ImBack, title: 'Facebook Auth', value: 'fblogin' }
        ];
        
        const card = CardFactory.heroCard('Please click below for any OAuth 2.0 samples (Bot Command - "auth")', undefined, buttons);
        return card;
    }
}

exports.AuthCardDialog = AuthCardDialog;