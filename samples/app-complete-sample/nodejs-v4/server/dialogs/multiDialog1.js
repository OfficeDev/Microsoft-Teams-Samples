// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const MULTIDIALOG1 = 'MultiDialog1';
class MultiDialog1 extends ComponentDialog {
    constructor(id) {
        super(id);

        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(MULTIDIALOG1, [
            this.beginMultiDialog1.bind(this),
        ]));
    }

    async beginMultiDialog1(stepContext) {
        await stepContext.context.sendActivity('Begin Multi Dailog 1');
        return await stepContext.endDialog();
    }
}

exports.MultiDialog1 = MultiDialog1;