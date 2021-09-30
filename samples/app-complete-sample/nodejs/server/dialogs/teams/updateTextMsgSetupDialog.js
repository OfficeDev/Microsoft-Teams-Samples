// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const SETUPTEXTMESSAGE = 'SetupTextMessage';

class UpdateTextMsgSetupDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(SETUPTEXTMESSAGE, [
            this.beginUpdateTextMsgSetupDialog.bind(this),
        ]));
    }

    async beginUpdateTextMsgSetupDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "UpdateTextMsgSetupDialog";
        var reply = stepContext.context._activity;
        reply.text = "Message set to be updated"

        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }

        var result = await stepContext.context.sendActivity(reply);
        currentState.activityId = result.id;
        return await stepContext.endDialog();
    }
}

exports.UpdateTextMsgSetupDialog = UpdateTextMsgSetupDialog;