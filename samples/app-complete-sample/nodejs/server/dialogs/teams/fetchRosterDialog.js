// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsInfo } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const FETCHROSTER = 'FetchRoster';

/**
 * FetchRosterDialog class extends ComponentDialog to handle fetching the roster of team members.
 */
class FetchRosterDialog extends ComponentDialog {
    /**
     * Constructor for the FetchRosterDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(FETCHROSTER, [
            this.beginFetchRosterDialog.bind(this),
        ]));
    }

    /**
     * Begins the fetch roster dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginFetchRosterDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "FetchRosterDialog";
        const members = await TeamsInfo.getMembers(stepContext.context);
        const reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }
        
        reply.text = JSON.stringify(members);
        await stepContext.context.sendActivity(reply.text);
        return await stepContext.endDialog();
    }
}

exports.FetchRosterDialog = FetchRosterDialog;