// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const HELLO = 'Hello';

/**
 * HelloDialog class extends ComponentDialog to handle a simple hello interaction.
 */
class HelloDialog extends ComponentDialog {
    /**
     * Constructor for the HelloDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(HELLO, [
            this.beginHelloDialog.bind(this),
        ]));
    }

    /**
     * Begins the hello dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginHelloDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "HelloDialog";
        await stepContext.context.sendActivity('This is Hello Dialog');
        return await stepContext.endDialog();
    }
}

exports.HelloDialog = HelloDialog;