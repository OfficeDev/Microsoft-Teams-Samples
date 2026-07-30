// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory, ActionTypes } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const MULTIDIALOG2 = 'MultiDialog2';

/**
 * MultiDialog2 class extends ComponentDialog to handle multi-dialog interactions.
 */
class MultiDialog2 extends ComponentDialog {
    /**
     * Constructor for the MultiDialog2 class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(MULTIDIALOG2, [
            this.beginMultiDialog2Dialog.bind(this),
        ]));
    }

    /**
     * Begins the multi-dialog 2.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginMultiDialog2Dialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "MultiDialog2";
        const reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }
        
        const buttons = [
            { type: ActionTypes.ImBack, title: 'Invoke Hello Dialog', value: 'hi' },
            { type: ActionTypes.ImBack, title: 'Invoke Multi Dialog', value: 'multi dialog 1' },
        ];
        const cardImage = CardFactory.images(["https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg"]);
        const card = CardFactory.heroCard(
            'Multi Dialog Card Title.',
            cardImage,
            buttons,
            { subtitle: "Multi Dialog Card Sub Title.", text: "Multi Dialog Card Text" }
        );

        reply.attachments = [card];
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }
}

exports.MultiDialog2 = MultiDialog2;