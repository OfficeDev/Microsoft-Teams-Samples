// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { CardFactory, ActionTypes } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const MULTIDIALOG2 = 'MultiDialog2';

class MultiDialog2 extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(MULTIDIALOG2, [
            this.beginMultiDialog2Dialog.bind(this),
        ]));
    }

    async beginMultiDialog2Dialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "MultiDialog2";
        var reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }
        
        const buttons = [
            { type: ActionTypes.ImBack, title: 'Invoke Hello Dialog', value: 'hi' },
            { type: ActionTypes.ImBack, title: 'Invoke Multi Dialog', value: 'multi dialog 1' },
        ];
        const CardImage = CardFactory.images(["https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg"])
        const card = CardFactory.heroCard('Multi Dialog Card Title.', CardImage,
            buttons, { subtitle: "Multi Dialog Card Sub Title.", text: "Multi Dialog Card Text" });

        reply.attachments = [card];
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }
}

exports.MultiDialog2 = MultiDialog2;