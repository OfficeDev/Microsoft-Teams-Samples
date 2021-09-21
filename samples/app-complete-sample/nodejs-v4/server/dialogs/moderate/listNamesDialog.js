// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { TeamsInfo, CardFactory, ActionTypes } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const LISTNAMES = 'ListNames';

class ListNamesDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(LISTNAMES, [
            this.beginListNamesDialog.bind(this),
        ]));
    }

    async beginListNamesDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "ListNamesDialog";
        var members = await TeamsInfo.getMembers(stepContext.context);
        var reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }

        reply.text = JSON.stringify(members)
        var card = [];

        if (members.length != 0) {
            for (let i = 0; i < members.length; i++) {
                card.push(this.getInformationCard(members[i].givenName + members[i].surname, members[i].aadObjectId));
            }
        }
        reply.attachments = card
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }

    getInformationCard(name, aadId) {
        var chatUrl = "https://teams.microsoft.com/l/chat/0/0?users=" + aadId;
        const buttons = [
            { type: ActionTypes.OpenUrl, title: 'Chat', value: chatUrl },
        ];
        const card = CardFactory.heroCard(name, undefined,
            buttons);
        return card;
    }
}

exports.ListNamesDialog = ListNamesDialog;