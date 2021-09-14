// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { TeamsInfo } = require('botbuilder');
const {WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const FETCHROSTER = 'FetchRoster';
class FetchRosterDialog extends ComponentDialog {
    constructor(id) {
        super(id);

        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(FETCHROSTER, [
            this.beginFetchRosterDialog.bind(this),
        ]));
    }

    async beginFetchRosterDialog(stepContext) {
        var members = await TeamsInfo.getMembers(stepContext.context);
        var reply = stepContext.context._activity;
        reply.text = JSON.stringify(members)
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }
}

exports.FetchRosterDialog = FetchRosterDialog;