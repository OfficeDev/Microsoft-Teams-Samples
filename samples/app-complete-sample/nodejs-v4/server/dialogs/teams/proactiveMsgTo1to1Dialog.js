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
        var userId = stepContext.context._activity.from.id;
        var botId = stepContext.context._activity.recipient.id;
        var botName = stepContext.context._activity.recipient.name;
        var channelData = stepContext.context._activity.channelData;

        var parameters ={
            "bot":{
                "id":botId,
                "name":botName
            },
            "members":[{
                "id":userId
            }],
            "channelData":channelData
        }

            return await stepContext.endDialog();
    }
}

exports.ProactiveMsgTo1to1Dialog = ProactiveMsgTo1to1Dialog;