// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { TeamsInfo ,CardFactory,ActionTypes} = require('botbuilder');
const {WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const LISTNAMES = 'ListNames';
class ListNamesDialog extends ComponentDialog {
    constructor(id) {
        super(id);

        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(LISTNAMES, [
            this.beginListNamesDialog.bind(this),
        ]));
    }

    async beginListNamesDialog(stepContext) {
        var members = await TeamsInfo.getMembers(stepContext.context);
        var reply = stepContext.context._activity;
        reply.text = JSON.stringify(members)
        if(members.length!=0){
            for(let i=0;i<members.length;i++){
                reply.attachments = [this.getInformationCard(members[i].givenName+members[i].surname,members[i].aadObjectId)]
            }
        }
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }

    getInformationCard(name,aadId){
        var chatUrl = "https://teams.microsoft.com/l/chat/0/0?users=" + aadId;
        const buttons = [
            { type: ActionTypes.OpenUrl, title: 'Chat', value:chatUrl},

        ];
        const card = CardFactory.heroCard(name, undefined,
        buttons);
            return card;
    }

}

exports.ListNamesDialog = ListNamesDialog;