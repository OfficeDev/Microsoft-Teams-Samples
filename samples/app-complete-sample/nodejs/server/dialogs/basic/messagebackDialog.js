// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const MESSAGEBACK = 'msgback';

/**
 * MessageBackDialog class extends ComponentDialog to handle message back interactions.
 */
class MessageBackDialog extends ComponentDialog {
    /**
     * Constructor for the MessageBackDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(MESSAGEBACK, [
            this.messageBackDialog.bind(this),
        ]));
    }

    /**
     * Begins the message back dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async messageBackDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "MessageBackReceiverDialog";
        await stepContext.context.sendActivity('This is Message Back example');
        return await stepContext.endDialog();
    }
}

exports.MessageBackDialog = MessageBackDialog;