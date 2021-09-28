// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { CardFactory, ActionTypes } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const UPDATECARDSETUP = 'UpdatecardSetupMsg';
class UpdateCardMsgSetupDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(UPDATECARDSETUP, [
            this.beginUpdateCardMsgSetupDialog.bind(this),
        ]));
    }

    async beginUpdateCardMsgSetupDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "UpdateCardMsgSetupDialog";
        var reply = stepContext.context._activity;

        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }

        const buttons = [
            { type: ActionTypes.MessageBack, title: 'Update Card', value: 'update card message', text: "update card message" },
        ];
        const CardImage = CardFactory.images(["https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg"])
        const card = CardFactory.heroCard('Title', CardImage,
            buttons, { subtitle: "Subtitle" });

        reply.attachments = [card];
        await stepContext.context.sendActivity(reply);
        currentState.activityId = reply.id;
        
        return await stepContext.endDialog();
    }
}

exports.UpdateCardMsgSetupDialog = UpdateCardMsgSetupDialog;