// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const MULTIDIALOG1 = 'MultiDialog1';

/**
 * MultiDialog1 class extends ComponentDialog to handle multi-dialog interactions.
 */
class MultiDialog1 extends ComponentDialog {
    /**
     * Constructor for the MultiDialog1 class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(MULTIDIALOG1, [
            this.beginMultiDialog1.bind(this),
        ]));
    }

    /**
     * Begins the multi-dialog 1.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginMultiDialog1(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "MultiDialog1";
        await stepContext.context.sendActivity('Begin Multi Dialog 1');
        return await stepContext.endDialog();
    }
}

exports.MultiDialog1 = MultiDialog1;