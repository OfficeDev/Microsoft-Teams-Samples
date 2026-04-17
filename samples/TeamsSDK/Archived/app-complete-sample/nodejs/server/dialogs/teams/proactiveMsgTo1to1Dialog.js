// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TurnContext } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const PROACTIVEMESSAGE = 'ProactiveMessage';
const conversationReferences = {};

/**
 * ProactiveMsgTo1to1Dialog class extends ComponentDialog to handle sending proactive messages to 1:1 conversations.
 */
class ProactiveMsgTo1to1Dialog extends ComponentDialog {
    /**
     * Constructor for the ProactiveMsgTo1to1Dialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(PROACTIVEMESSAGE, [
            this.beginProactiveMsgTo1to1Dialog.bind(this),
        ]));
    }

    /**
     * Begins the proactive message to 1:1 dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginProactiveMsgTo1to1Dialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "ProactiveMsgTo1to1Dialog";
        await stepContext.context.sendActivity("1:1 Message sent");
        this.addConversationReference(stepContext.context._activity);
        const reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }
        
        reply.text = "Hey! I am Bot, How's going!!";
        await stepContext.context.sendActivity(reply.text);
        
        return await stepContext.endDialog();
    }

    /**
     * Adds the conversation reference for proactive messaging.
     * @param {Activity} activity - The activity from which to extract the conversation reference.
     */
    addConversationReference(activity) {
        const conversationReference = TurnContext.getConversationReference(activity);
        conversationReferences[conversationReference.conversation.id] = conversationReference;
    }
}

exports.ProactiveMsgTo1to1Dialog = ProactiveMsgTo1to1Dialog;