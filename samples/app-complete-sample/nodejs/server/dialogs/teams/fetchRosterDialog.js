// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { TeamsInfo } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const FETCHROSTER = 'FetchRoster';

class FetchRosterDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(FETCHROSTER, [
            this.beginFetchRosterDialog.bind(this),
        ]));
    }

    async beginFetchRosterDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "FetchRosterDialog";
        var members = await TeamsInfo.getMembers(stepContext.context);
        var reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }
        
        reply.text = JSON.stringify(members)
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }
}

exports.FetchRosterDialog = FetchRosterDialog;