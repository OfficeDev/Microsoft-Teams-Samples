// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const BEGINDIALOG = 'BeginDialog';
const HELLO = 'Hello';
const { HelloDialog } = require('../basic/helloDialog');

/**
 * BeginDialogExampleDialog class extends ComponentDialog to handle a simple dialog flow example.
 */
class BeginDialogExampleDialog extends ComponentDialog {
    /**
     * Constructor for the BeginDialogExampleDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(BEGINDIALOG, [
            this.beginBeginDialogExampleDialog.bind(this),
            this.continueBeginDialogExampleDialog.bind(this),
        ]));
        this.addDialog(new HelloDialog(HELLO, this.conversationDataAccessor));
    }

    /**
     * Begins the begin dialog example.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginBeginDialogExampleDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "BeginDialogFlowDialog";
        await stepContext.context.sendActivity('Hello, welcome to begin dialog');
        return await stepContext.beginDialog(HELLO);
    }

    /**
     * Continues the begin dialog example.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async continueBeginDialogExampleDialog(stepContext) {
        await stepContext.context.sendActivity('Begin dialog end');
        return await stepContext.endDialog();
    }
}

exports.BeginDialogExampleDialog = BeginDialogExampleDialog;