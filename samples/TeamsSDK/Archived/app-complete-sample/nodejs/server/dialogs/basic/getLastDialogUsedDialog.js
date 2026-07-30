// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const GETLASTDIALOG = 'lastDialog';

/**
 * GetLastDialogUsedDialog class extends ComponentDialog to handle retrieving the last used dialog.
 */
class GetLastDialogUsedDialog extends ComponentDialog {
    /**
     * Constructor for the GetLastDialogUsedDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(GETLASTDIALOG, [
            this.beginGetLastDialogUsedDialog.bind(this),
        ]));
    }

    /**
     * Begins the get last dialog used dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginGetLastDialogUsedDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        
        if (currentState.lastDialogKey != null) {
            await stepContext.context.sendActivity('Last response was from dialog: ' + currentState.lastDialogKey);
        } else {
            currentState.lastDialogKey = "GetLastDialogUsedDialog";
            await stepContext.context.sendActivity('Error finding last executed dialog');
        }
        return await stepContext.endDialog();
    }
}

exports.GetLastDialogUsedDialog = GetLastDialogUsedDialog;