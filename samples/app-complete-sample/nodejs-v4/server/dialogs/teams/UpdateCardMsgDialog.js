// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory, ActionTypes } = require('botbuilder');
const {WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const UPDATECARDMESSAGE = 'UpdateCardMessage';
class UpdateCardMsgDialog extends ComponentDialog {
    constructor(id,conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(UPDATECARDMESSAGE, [
            this.beginUpdateCardMsgDialog.bind(this),
        ]));
    }

    async beginUpdateCardMsgDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        if(currentState.lastDialogKey=="UpdateCardMsgSetupDialog" && currentState.activityId!=null){
            var reply = stepContext.context._activity;
            reply.id = currentState.activityId;
            reply.replyToId = currentState.activityId;
            if(reply.attachments != null && reply.entities.length>1){
                reply.attachments = null;
                reply.entities.splice(0,1);
            }
            const buttons = [
                { type: ActionTypes.MessageBack, title: 'MSDN', value: 'update card message', text: "update card message" },
    
            ];
            const CardImage = CardFactory.images(["https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg"])
            const card = CardFactory.heroCard('Updated card ', CardImage,
                buttons, { subtitle: "This card is updated now"});
    
            reply.attachments = [card];
            await stepContext.context.updateActivity(reply);
            await stepContext.context.sendActivity("Message updated - see above");
            currentState.lastDialogKey = "UpdateCardMsgDialog";
        }
        else{
            currentState.lastDialogKey = "UpdateCardMsgDialog";
            await stepContext.context.sendActivity("No message to update.");
        }
        
        
        return await stepContext.endDialog();
    }
}

exports.UpdateCardMsgDialog = UpdateCardMsgDialog;