// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { CardFactory, ActionTypes } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const THUMBNAILCARD = 'ThumbnailCard';

class ThumbnailCardDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(THUMBNAILCARD, [
            this.beginThumbnailCardDialog.bind(this),
        ]));
    }

    async beginThumbnailCardDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "ThumbnailCardDialog";
        var reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }

        const buttons = [
            { type: ActionTypes.OpenUrl, title: 'Get Started', value: "https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-add-rich-card-attachments" },
            { type: ActionTypes.MessageBack, title: 'Message Back', value: 'msgback', text: "msgback", displayText: "This is Displayed Text for Message Back" },
        ];
        
        const CardImage = CardFactory.images(["https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg"])
        const card = CardFactory.thumbnailCard('BotFramework Thumbnail Card', CardImage,
            buttons, { subtitle: "Your bots — wherever your users are talking", text: "Build and connect intelligent bots to interact with your users naturally wherever they are, from text/sms to Skype, Slack, Office 365 mail and other popular services." });

        reply.attachments = [card];
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }
}

exports.ThumbnailCardDialog = ThumbnailCardDialog;