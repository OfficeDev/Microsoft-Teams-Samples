// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory, ActionTypes } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const UPDATECARDSETUP = 'UpdatecardSetupMsg';

/**
 * UpdateCardMsgSetupDialog class extends ComponentDialog to handle setting up card messages for updates.
 */
class UpdateCardMsgSetupDialog extends ComponentDialog {
    /**
     * Constructor for the UpdateCardMsgSetupDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(UPDATECARDSETUP, [
            this.beginUpdateCardMsgSetupDialog.bind(this),
        ]));
    }

    /**
     * Begins the update card message setup dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginUpdateCardMsgSetupDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "UpdateCardMsgSetupDialog";
        const reply = stepContext.context._activity;

        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }

        const buttons = [
            { type: ActionTypes.MessageBack, title: 'Update Card', value: 'update card message', text: "update card message" },
        ];
        const cardImage = CardFactory.images(["https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg"]);
        const card = CardFactory.heroCard('Title', cardImage, buttons, { subtitle: "Subtitle" });

        reply.attachments = [card];
        await stepContext.context.sendActivity(reply);
        currentState.activityId = reply.id;

        return await stepContext.endDialog();
    }
}

exports.UpdateCardMsgSetupDialog = UpdateCardMsgSetupDialog;