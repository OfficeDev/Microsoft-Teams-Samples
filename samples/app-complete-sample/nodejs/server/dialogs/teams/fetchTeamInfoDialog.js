// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsInfo } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const FETCHTEAMINFO = 'FetchTeamInfo';

/**
 * FetchTeamInfoDialog class extends ComponentDialog to handle fetching the team information.
 */
class FetchTeamInfoDialog extends ComponentDialog {
    /**
     * Constructor for the FetchTeamInfoDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(FETCHTEAMINFO, [
            this.beginFetchTeamInfoDialog.bind(this),
        ]));
    }

    /**
     * Begins the fetch team info dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginFetchTeamInfoDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "FetchTeamInfoDialog";
        const teamId = stepContext.context._activity.channelData.teamsTeamId;

        if (teamId != null) {
            const teamDetails = await TeamsInfo.getTeamDetails(stepContext.context, teamId);
            const reply = stepContext.context._activity;
            if (reply.attachments != null && reply.entities.length > 1) {
                reply.attachments = null;
                reply.entities.splice(0, 1);
            }

            reply.text = this.generateTableForTeamInfo(teamDetails);
            await stepContext.context.sendActivity(reply);
            return await stepContext.endDialog();
        } else {
            await stepContext.context.sendActivity("This command only works in channels");
            return await stepContext.endDialog();
        }
    }

    /**
     * Generates the team info data in table format.
     * @param {TeamDetails} teamDetails - The team details.
     * @returns {string} The formatted team info.
     */
    generateTableForTeamInfo(teamDetails) {
        if (teamDetails) {
            // Currently, aadGroupId is present but is not defined in the TeamInfo typings
            return `Team id : ${teamDetails.id},
                    Team name : ${teamDetails.name},
                    AAD group id : ${teamDetails.aadGroupId}`;
        }

        return "";
    }
}

exports.FetchTeamInfoDialog = FetchTeamInfoDialog;