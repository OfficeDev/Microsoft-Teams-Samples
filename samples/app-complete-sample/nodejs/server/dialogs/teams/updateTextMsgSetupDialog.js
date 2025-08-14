// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const SETUPTEXTMESSAGE = 'SetupTextMessage';

/**
 * UpdateTextMsgSetupDialog class extends ComponentDialog to handle setting up text messages for updates.
 */
class UpdateTextMsgSetupDialog extends ComponentDialog {
    /**
     * Constructor for the UpdateTextMsgSetupDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(SETUPTEXTMESSAGE, [
            this.beginUpdateTextMsgSetupDialog.bind(this),
        ]));
    }

    /**
     * Begins the update text message setup dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginUpdateTextMsgSetupDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "UpdateTextMsgSetupDialog";
        const reply = stepContext.context._activity;
        reply.text = "Message set to be updated";

        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }

        const result = await stepContext.context.sendActivity(reply.text);
        currentState.activityId = result.id;
        return await stepContext.endDialog();
    }
}

exports.UpdateTextMsgSetupDialog = UpdateTextMsgSetupDialog;