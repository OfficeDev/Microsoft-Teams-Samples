// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { MessageFactory } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const TextEncoder = require('util').TextEncoder;
const ATMENTION = 'AtMention';

/**
 * AtMentionDialog class extends ComponentDialog to handle @mention interactions.
 */
class AtMentionDialog extends ComponentDialog {
    /**
     * Constructor for the AtMentionDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(ATMENTION, [
            this.beginAtMentionDialog.bind(this),
        ]));
    }

    /**
     * Begins the @mention dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginAtMentionDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
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