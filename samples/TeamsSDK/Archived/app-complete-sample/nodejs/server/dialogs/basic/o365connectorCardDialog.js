// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const O365CONNECTORECARD = 'O365ConnectorCard';

/**
 * O365ConnectorCardDialog class extends ComponentDialog to handle O365 connector card actions.
 */
class O365ConnectorCardDialog extends ComponentDialog {
    /**
     * Constructor for the O365ConnectorCardDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(O365CONNECTORECARD, [
            this.beginO365ConnectorCardDialog.bind(this),
        ]));
    }

    /**
     * Begins the O365 connector card dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginO365ConnectorCardDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "O365ConnectorCardDialog";
        const text = stepContext.context._activity.text.trim();
        const inputNumber = text.substr(text.length - 1, 1);
        const reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }
        
        let card;
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
            default:
                card = this.O365ConnectorCardDefault();
                break;
        }

        reply.attachments = [card];
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }

    /**
     * Creates and returns the default O365 connector card.
     * @returns {Attachment} The O365 connector card attachment.
     */
    O365ConnectorCardDefault() {
        return CardFactory.o365ConnectorCard({
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
    }

    /**
     * Creates and returns the O365 connector card with facts in section.
     * @returns {Attachment} The O365 connector card attachment.
     */
    O365ConnectorCardFactsInSection() {
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

    /**
     * Creates and returns the O365 connector card with image in section.
     * @returns {Attachment} The O365 connector card attachment.
     */
    O365ConnectorCardImageInSection() {
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