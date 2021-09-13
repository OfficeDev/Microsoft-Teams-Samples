// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { CardFactory,ActionTypes } = require('botbuilder');
const {WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const POPUPSIGNINCARD = 'PopupSignInCard';
class PopupSigninCardDialog  extends ComponentDialog {
    constructor(id) {
        super(id);

        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(POPUPSIGNINCARD, [
            this.beginPopupSigninCardDialog.bind(this),
        ]));
    }

    async beginPopupSigninCardDialog(stepContext) {
        var reply = stepContext.context._activity;
        const buttons = [
            { type: ActionTypes.Signin, title: 'Sign In', value: process.env.BaseUri+"/tab/tabConfig/popUpSignin.html?height=400&width=400" },

        ];

        const card = CardFactory.heroCard('Please click below for Popup Sign-In experience', undefined,
        buttons);

    reply.attachments = [card];
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }
}

exports.PopupSigninCardDialog = PopupSigninCardDialog;