// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const ATMENTION = 'AtMention';
class AtMentionDialog extends ComponentDialog {
    constructor(id,conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(ATMENTION, [
            this.beginAtMentionDialog.bind(this),
        ]));
    }

    async beginAtMentionDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "AtMentionDialog";
        var message = stepContext.context._activity;
        message.text = "at mention"
        await stepContext.context.sendActivity(message);
        return await stepContext.endDialog();
    }
}

exports.AtMentionDialog = AtMentionDialog;