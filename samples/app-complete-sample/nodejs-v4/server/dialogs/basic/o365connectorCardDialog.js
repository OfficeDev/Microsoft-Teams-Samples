// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { CardFactory } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const O365CONNECTORECARD = 'O365ConnectorCard';

class O365ConnectorCardDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(O365CONNECTORECARD, [
            this.beginO365ConnectorCardDialog.bind(this),
        ]));
    }

    async beginO365ConnectorCardDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "O365ConnectorCardDialog";
        var text = stepContext.context._activity.text.trim();
        var inputNumber = text.substr(text.length - 1, 1);
        var reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }
        
        var card;
        switch (inputNumber) {
            case "1":
                card = this.O365ConnectorCardDefault();
                break;
            case "2":
                card = this.O365ConnectorCardFactsInSection();
                break;
            case "3":
                card = this.O365ConnectorCardImageInSection();
                break;
        }

        reply.attachments = [card];
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }

    O365ConnectorCardDefault = () => {

        var card = CardFactory.o365ConnectorCard({
            "title": "Title",
            "sections": [
                {
                    "text": "This is the text1"
                },
                {
                    "text": "This is the text2"
                }
            ]
        });
        return card;
    }

    O365ConnectorCardFactsInSection = () => {

        return CardFactory.o365ConnectorCard({
            "themeColor": "#fe9a13",
            "sections": [
                {
                    "title": "**New major event on omi10svr**",
                    "activityTitle": "Batch upload for TAX data on db-srv-hr1 aborted due to timeout. (ref324)",
                    "text": "This is the text1",
                    "facts": [
                        {
                            "name": "Receive Time",
                            "value": "2016-05-30T16:50:02.503Z"
                        },
                        {
                            "name": "Node",
                            "value": "omi10svr"
                        },
                        {
                            "name": "Category",
                            "value": "job"
                        },
                        {
                            "name": "Priority",
                            "value": "medium"
                        }
                    ]
                }
            ]
        });
    }

    O365ConnectorCardImageInSection = () => {

        return CardFactory.o365ConnectorCard({
            "themeColor": "fe9a13",
            "title": "Issue opened: Push notifications not working",
            "summary": "Issue 176715375",
            "sections": [
                {
                    "activityTitle": "Miguel Garcie",
                    "activitySubtitle": "9/13/2016, 11:46am",
                    "activityImage": "http://connectorsdemo.azurewebsites.net/images/MSC12_Oscar_002.jpg",
                    "facts": [
                        {
                            "name": "Repository:",
                            "value": "mgarcia\\test"
                        },
                        {
                            "name": "Issue #:",
                            "value": "176715375"
                        }
                    ],
                    "text": "There is a problem with Push notifications, they don't seem to be picked up by the connector."
                }
            ]
        });
    }
}

exports.O365ConnectorCardDialog = O365ConnectorCardDialog;