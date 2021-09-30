// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { TeamsInfo } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const FETCHTEAMINFO = 'FetchTeamInfo'
class FetchTeamInfoDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(FETCHTEAMINFO, [
            this.beginFetchTeamInfoDialog.bind(this),
        ]));
    }

    async beginFetchTeamInfoDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "FetchTeamInfoDialog";
        var teamId = stepContext.context._activity.channelData.teamsTeamId;      
        if (teamId != null) {
            var teamDetails = await TeamsInfo.getTeamDetails(stepContext.context, teamId)
            var reply = stepContext.context._activity;
            if (reply.attachments != null && reply.entities.length > 1) {
                reply.attachments = null;
                reply.entities.splice(0, 1);
            }
            
            reply.text = this.generateTableForTeamInfo(teamDetails);
            await stepContext.context.sendActivity(reply);
            return await stepContext.endDialog();
        }
        else {
            await stepContext.context.sendActivity("This command only works in channels");
            return await stepContext.endDialog();
        }
    }

    // Generate the team info data in table format
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