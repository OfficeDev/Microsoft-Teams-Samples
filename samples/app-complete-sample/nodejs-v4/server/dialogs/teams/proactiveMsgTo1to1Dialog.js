// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { TeamsInfo } = require('botbuilder');
const {WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const PROACTIVEMESSAGE = 'ProactiveMessage';
class ProactiveMsgTo1to1Dialog extends ComponentDialog {
    constructor(id,conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(PROACTIVEMESSAGE, [
            this.beginProactiveMsgTo1to1Dialog.bind(this),
        ]));
    }

    async beginProactiveMsgTo1to1Dialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "ProactiveMsgTo1to1Dialog";
        await stepContext.context.sendActivity("1:1 Message sent");
        var teamId = TeamsInfo.getTeamId;
        var teamDetails = await TeamsInfo.getTeamDetails(stepContext.context,teamId)
        if(teamDetails!=null){
            var reply = stepContext.context._activity;
            reply.text = this.generateTableForTeamInfo(teamDetails);
            await stepContext.context.sendActivity(reply);
            return await stepContext.endDialog();
        }
        else
        {
            await stepContext.context.sendActivity("This command only works in channels");
            return await stepContext.endDialog();
        }
    }

    // Generate the team info data in table format
    generateTableForTeamInfo(teamDetails){
        if (teamDetails) {
            // Currently, aadGroupId is present but is not defined in the TeamInfo typings
            return `<table border='1'>
                        <tr><td> Team id </td><td>${teamDetails.id}</td></tr>
                        <tr><td> Team name </td><td>${teamDetails.name}</td></tr>
                        <tr><td> AAD group id </td><td>${teamDetails.aadGroupId}</td><tr>
                    </table>`;
        }
        return "";
    }
}

exports.ProactiveMsgTo1to1Dialog = ProactiveMsgTo1to1Dialog;