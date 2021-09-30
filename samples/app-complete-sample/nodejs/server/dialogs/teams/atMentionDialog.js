// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { MessageFactory } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const TextEncoder = require('util').TextEncoder;
const ATMENTION = 'AtMention';

class AtMentionDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
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
        const mention = {
            mentioned: stepContext.context._activity.from,
            text: `<at>${new TextEncoder().encode(
                stepContext.context._activity.from.name
            )}</at>`,
            type: 'mention',
        };

        const replyActivity = MessageFactory.text(`Hi ${mention.text}`);
        replyActivity.entities = [mention];
        await stepContext.context.sendActivity(replyActivity);
        return await stepContext.endDialog();
    }
}

exports.AtMentionDialog = AtMentionDialog;