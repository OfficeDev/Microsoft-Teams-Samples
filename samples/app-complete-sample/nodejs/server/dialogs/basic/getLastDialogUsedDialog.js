// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const GETLASTDIALOG = 'lastDialog';

class GetLastDialogUsedDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(GETLASTDIALOG, [
            this.beginGetLastDialogUsedDialog.bind(this),
        ]));
    }

    async beginGetLastDialogUsedDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        
        if (currentState.lastDialogKey != null) {
            await stepContext.context.sendActivity('Last response was from dialog: ' + currentState.lastDialogKey);
        }
        else {
            currentState.lastDialogKey = "GetLastDialogUsedDialog"
            await stepContext.context.sendActivity('Error finding last executed dialog');
        }
        return await stepContext.endDialog();
    }
}

exports.GetLastDialogUsedDialog = GetLastDialogUsedDialog;