// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { CardFactory, ActionTypes } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const HELP = 'Help';

class HelpDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(HELP, [
            this.beginHelpDialog.bind(this),
        ]));
    }

    async beginHelpDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "HelpDialog";
        var reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);

        }
        const buttons = [
            { type: ActionTypes.ImBack, title: 'At Mention', value: 'at mention' },
            { type: ActionTypes.ImBack, title: 'RunQuiz', value: 'quiz' },
            { type: ActionTypes.ImBack, title: 'Fetch Members', value: 'names' },
            { type: ActionTypes.ImBack, title: 'Play Game', value: 'prompt' },
            { type: ActionTypes.ImBack, title: 'Fetch Roster Payload', value: 'roster' },
            { type: ActionTypes.ImBack, title: 'Dialog Flow', value: 'dialog flow' },
            { type: ActionTypes.ImBack, title: 'Hello Dialog', value: 'hi' },
            { type: ActionTypes.ImBack, title: 'Begin Multi Dailog 1', value: 'multi dialog 1' },
            { type: ActionTypes.ImBack, title: 'Begin Multi Dialog 2', value: 'multi dialog 2' },
            { type: ActionTypes.ImBack, title: 'Fetch Last Dialog', value: 'last dialog' },
            { type: ActionTypes.ImBack, title: 'Setup Message', value: 'setup text message' },
            { type: ActionTypes.ImBack, title: 'Update Message', value: 'update text message' },
            { type: ActionTypes.ImBack, title: 'Send 1:1 Message', value: 'send message to 1:1' },
            { type: ActionTypes.ImBack, title: 'Update Card', value: 'setup card message' },
            { type: ActionTypes.ImBack, title: 'Choose Cards', value: 'display cards' },
            { type: ActionTypes.ImBack, title: 'Tab Example', value: 'deep link' },
            { type: ActionTypes.ImBack, title: 'Auth Sample', value: 'auth' },
            { type: ActionTypes.ImBack, title: 'Local Time', value: 'timezone' },
            { type: ActionTypes.ImBack, title: 'Message Back', value: 'msgback' },
            { type: ActionTypes.ImBack, title: 'Popup Sign-In', value: 'signin' },
            { type: ActionTypes.ImBack, title: 'Team Info', value: 'team info' },
        ];
        
        const card = CardFactory.heroCard('Template Options', undefined,
            buttons);

        reply.attachments = [card];
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }
}

exports.HelpDialog = HelpDialog;