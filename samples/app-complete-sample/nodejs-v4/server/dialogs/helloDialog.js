// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const HELLO = 'Hello';
class HelloDialog extends ComponentDialog {
    constructor(id) {
        super(id);

        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(HELLO, [
            this.beginHelloDialog.bind(this),
        ]));
    }

    async beginHelloDialog(stepContext) {
        await stepContext.context.sendActivity('This is Hello Dialog');
        return await stepContext.endDialog();
    }
}

exports.HelloDialog = HelloDialog;