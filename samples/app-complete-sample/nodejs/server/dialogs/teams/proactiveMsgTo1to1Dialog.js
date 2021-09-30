// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { TurnContext } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const PROACTIVEMESSAGE = 'ProactiveMessage';
const conversationReferences = {};

class ProactiveMsgTo1to1Dialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(PROACTIVEMESSAGE, [
            this.beginProactiveMsgTo1to1Dialog.bind(this),
        ]));
    }

    async beginProactiveMsgTo1to1Dialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "ProactiveMsgTo1to1Dialog";
        await stepContext.context.sendActivity("1:1 Message sent");
        this.addConversationReference(stepContext.context._activity);
        var reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }
        
        reply.text = "Hey! I am Bot, How's going!!"
        await stepContext.context.sendActivity(reply);
        
        return await stepContext.endDialog();
    }

    addConversationReference(activity) {
        const conversationReference = TurnContext.getConversationReference(activity);
        conversationReferences[conversationReference.conversation.id] = conversationReference;
    }
}

exports.ProactiveMsgTo1to1Dialog = ProactiveMsgTo1to1Dialog;