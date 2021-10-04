// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const HELLO = 'Hello';

class HelloDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(HELLO, [
            this.beginHelloDialog.bind(this),
        ]));
    }

    async beginHelloDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "HelloDialog";
        await stepContext.context.sendActivity('This is Hello Dialog');
        return await stepContext.endDialog();
    }
}

exports.HelloDialog = HelloDialog;