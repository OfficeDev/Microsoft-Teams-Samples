// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const MESSAGEBACK = 'msgback';
class MessageBackDialog extends ComponentDialog {
    constructor(id) {
        super(id);

        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(MESSAGEBACK, [
            this.MessageBackDialog.bind(this),
        ]));
    }

    async MessageBackDialog(stepContext) {
        await stepContext.context.sendActivity('This is Message Back example');
        return await stepContext.endDialog();
    }
}

exports.MessageBackDialog = MessageBackDialog;