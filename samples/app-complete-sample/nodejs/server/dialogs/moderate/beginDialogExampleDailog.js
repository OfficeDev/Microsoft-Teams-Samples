// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const BEGINdIALOG = 'BeginDialog';
const HELLO = 'Hello';
const { HelloDialog } = require('../basic/helloDialog');

class BeginDialogExampleDailog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(BEGINdIALOG, [
            this.beginBeginDialogExampleDailog.bind(this),
            this.continueBeginDialogExampleDailog.bind(this),
        ]));
        this.addDialog(new HelloDialog(HELLO,this.conversationDataAccessor));
    }

    async beginBeginDialogExampleDailog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "BeginDialogFlowDialog";
        await stepContext.context.sendActivity('Hello, welcome to begin dialog');
        return await stepContext.beginDialog(HELLO);
    }

    async continueBeginDialogExampleDailog(stepContext) {
        await stepContext.context.sendActivity('Begin dialog end');
        return await stepContext.endDialog();
    }
}

exports.BeginDialogExampleDailog = BeginDialogExampleDailog;