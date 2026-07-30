// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const UPDATETEXTMESSAGE = 'UpdateTextMessage';

/**
 * UpdateTextMsgDialog class extends ComponentDialog to handle updating text messages.
 */
class UpdateTextMsgDialog extends ComponentDialog {
    /**
     * Constructor for the UpdateTextMsgDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(UPDATETEXTMESSAGE, [
            this.beginUpdateTextMsgDialog.bind(this),
        ]));
    }

    /**
     * Begins the update text message dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginUpdateTextMsgDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
    
        if (currentState.lastDialogKey === "UpdateTextMsgSetupDialog" && currentState.activityId != null) {
            const reply = stepContext.context._activity;
            reply.id = currentState.activityId;
    
            if (reply.attachments != null && reply.entities.length > 1) {
                reply.attachments = null;
                reply.entities.splice(0, 1);
            }
            reply.text = "This message has been updated";
            await stepContext.context.updateActivity(reply);
            await stepContext.context.sendActivity("Message updated - see above");
            currentState.lastDialogKey = "UpdateTextMsgDialog";
        } else {
            currentState.lastDialogKey = "UpdateTextMsgDialog";
            await stepContext.context.sendActivity("Please setup card message using \"setup text message\" command before updating card.");
        }
    
        return await stepContext.endDialog();
    }
}

exports.UpdateTextMsgDialog = UpdateTextMsgDialog;