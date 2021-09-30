// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const UPDATETEXTMESSAGE = 'UpdateTextMessage';

class UpdateTextMsgDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(UPDATETEXTMESSAGE, [
            this.beginUpdateTextMsgDialog.bind(this),
        ]));
    }

    async beginUpdateTextMsgDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});

        if (currentState.lastDialogKey == "UpdateTextMsgSetupDialog" && currentState.activityId != null) {
            var reply = stepContext.context._activity;
            reply.id = currentState.activityId;

            if (reply.attachments != null && reply.entities.length > 1) {
                reply.attachments = null;
                reply.entities.splice(0, 1);
            }
            reply.text = "This message has been updated"
            await stepContext.context.updateActivity(reply);
            await stepContext.context.sendActivity("Message updated - see above");
            currentState.lastDialogKey = "UpdateTextMsgDialog";
        }
        else {
            currentState.lastDialogKey = "UpdateTextMsgDialog";
            await stepContext.context.sendActivity("Please setup card message using \"setup text message\" command before updating card.");
        }

        return await stepContext.endDialog();
    }
}

exports.UpdateTextMsgDialog = UpdateTextMsgDialog;