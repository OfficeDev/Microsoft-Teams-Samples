// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const ATMENTION = 'AtMention';
class AtMentionDialog extends ComponentDialog {
    constructor(id) {
        super(id);

        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(ATMENTION, [
            this.beginAtMentionDialog.bind(this),
        ]));
    }

    async beginAtMentionDialog(stepContext) {
        var message = stepContext.context._activity;
        let replyActivity = stepContext.context.getMentions(message);
        await stepContext.Context.SendActivityAsync(replyActivity);
        return await stepContext.endDialog();
    }
}

exports.AtMentionDialog = AtMentionDialog;