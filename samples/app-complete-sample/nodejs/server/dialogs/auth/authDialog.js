// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { CardFactory, ActionTypes } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const AUTHCARD = 'AuthCard';

class AuthCardDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(AUTHCARD, [
            this.beginAuthCardDialog.bind(this),
        ]));
    }

    async beginAuthCardDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "AuthCardDialog";
        var reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }
        reply.attachments = [this.createAuthSampleCard()];
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }

    createAuthSampleCard() {
        const buttons = [
            { type: ActionTypes.ImBack, title: 'Facebook Auth', value: 'fblogin' }
        ];
        
        const card = CardFactory.heroCard('Please Clicked below any OAuth 2.0 Samples (Bot Command - "auth")', undefined,
            buttons);
        return card;
    }
}

exports.AuthCardDialog = AuthCardDialog;