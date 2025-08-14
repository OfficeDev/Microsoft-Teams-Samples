// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const O365CONNECTORCARDACTION = 'O365ConnectorCardAction';

/**
 * O365ConnectorCardActionDialog class extends ComponentDialog to handle O365 connector card actions.
 */
class O365ConnectorCardActionDialog extends ComponentDialog {
    /**
     * Constructor for the O365ConnectorCardActionDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(O365CONNECTORCARDACTION, [
            this.beginO365ConnectorCardActionDialog.bind(this),
        ]));
    }

    /**
     * Begins the O365 connector card action dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginO365ConnectorCardActionDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "O365ConnectorCardActionsDialog";
        const text = stepContext.context._activity.text.trim();
        const inputNumber = text.substr(text.length - 1, 1);
        console.log(inputNumber);
        const reply = stepContext.context._activity;
        if (reply.attachments != null && reply.entities.length > 1) {
            reply.attachments = null;
            reply.entities.splice(0, 1);
        }
        let card;
        switch (inputNumber) {
            case "2":
                card = this.O365ActionableCardMultipleSection();
                break;
            case "":
            default:
                card = this.O365ActionableCardDefault();
                break;
        }

        reply.attachments = [card];
        await stepContext.context.sendActivity(reply);
        return await stepContext.endDialog();
    }

    /**
     * Creates and returns the default O365 actionable card.
     * @returns {Attachment} The O365 actionable card attachment.
     */
    O365ActionableCardDefault() {
        return CardFactory.o365ConnectorCard({
            "@context": "https://schema.org/extensions",
            "@type": "MessageCard",
            "themeColor": "#E67A9E",
            "title": "card title",
            "text": "card text",
            "summary": "O365 card summary",
            "sections": [
                {
                    "title": "**section title**",
                    "text": "section text",
                    "activityTitle": "activity title",
                    "activitySubtitle": "activity subtitle",
                    "activityText": "activity text",
                    "activityImage": "http://connectorsdemo.azurewebsites.net/images/MSC12_Oscar_002.jpg",
                    "markdown": true,
                    "facts": [
                        { "name": "Fact name 1", "value": "Fact value 1" },
                        { "name": "Fact name 2", "value": "Fact value 2" }
                    ],
                    "images": [
                        { "image": "http://connectorsdemo.azurewebsites.net/images/MicrosoftSurface_024_Cafe_OH-06315_VS_R1c.jpg", "title": "image 1" },
                        { "image": "http://connectorsdemo.azurewebsites.net/images/WIN12_Scene_01.jpg", "title": "image 2" },
                        { "image": "http://connectorsdemo.azurewebsites.net/images/WIN12_Anthony_02.jpg", "title": "image 3" }
                    ]
                }
            ],
            "potentialAction": [
                {
                    "@type": "ActionCard",
                    "name": "Multiple Choice",
                    "inputs": [
                        {
                            "@type": "multichoiceInput",
                            "id": "cardstype",
                            "isRequired": true,
                            "title": "Pick multiple options",
                            "choices": [
                                { "display": "Hero Card", "value": "Hero Card" },
                                { "display": "Thumbnail Card", "value": "Thumbnail Card" },
                                { "display": "O365 Connector Card", "value": "O365 Connector Card" }
                            ],
                            "style": "expanded",
                            "isMultiSelect": true
                        },
                        {
                            "@type": "multichoiceInput",
                            "id": "Teams",
                            "isRequired": true,
                            "title": "Pick multiple options",
                            "choices": [
                                { "display": "Bot", "value": "Bot" },
                                { "display": "Tab", "value": "Tab" },
                                { "display": "Connector", "value": "Connector" },
                                { "display": "Compose Extension", "value": "Compose Extension" }
                            ],
                            "style": "compact",
                            "isMultiSelect": true
                        },
                        {
                            "@type": "multichoiceInput",
                            "id": "Apps",
                            "isRequired": true,
                            "title": "Pick an App",
                            "choices": [
                                { "display": "VSTS", "value": "VSTS" },
                                { "display": "Wiki", "value": "Wiki" },
                                { "display": "Github", "value": "Github" }
                            ],
                            "style": "expanded",
                            "isMultiSelect": false
                        },
                        {
                            "@type": "multichoiceInput",
                            "id": "OfficeProduct",
                            "isRequired": true,
                            "title": "Pick an Office Product",
                            "choices": [
                                { "display": "Outlook", "value": "Outlook" },
                                { "display": "MS Teams", "value": "MS Teams" },
                                { "display": "Skype", "value": "Skype" }
                            ],
                            "style": "compact",
                            "isMultiSelect": false
                        }
                    ],
                    "actions": [
                        {
                            "@type": "HttpPOST",
                            "name": "send",
                            "body": "cardstype={{cardstype.value}},Teams={{Teams.value}},Apps={{Apps.value}},OfficeProduct={{OfficeProduct.value}}"
                        }
                    ]
                },
                {
                    "@type": "ActionCard",
                    "name": "Input Card",
                    "inputs": [
                        {
                            "@type": "textInput",
                            "id": "text-1",
                            "isRequired": false,
                            "title": "multiline, no maxLength",
                            "isMultiline": true
                        },
                        {
                            "@type": "textInput",
                            "id": "text-2",
                            "isRequired": false,
                            "title": "single line, no maxLength",
                            "isMultiline": false
                        },
                        {
                            "@type": "textInput",
                            "id": "text-3",
                            "isRequired": false,
                            "title": "multiline, max len = 10, isRequired",
                            "isMultiline": true,
                            "maxLength": 10
                        },
                        {
                            "@type": "textInput",
                            "id": "text-4",
                            "isRequired": false,
                            "title": "single line, max len = 10, isRequired",
                            "isMultiline": false
                        }
                    ],
                    "actions": [
                        {
                            "@type": "HttpPOST",
                            "name": "Send",
                            "body": "text1={{text-1.value}}, text2={{text-2.value}},text3={{text-3.value}},text4={{text-4.value}}"
                        }
                    ]
                },
                {
                    "@type": "ActionCard",
                    "name": "Date Input",
                    "inputs": [
                        {
                            "@type": "dateInput",
                            "id": "date-1",
                            "isRequired": true,
                            "title": "date with time",
                            "includeTime": true
                        },
                        {
                            "@type": "dateInput",
                            "id": "date-2",
                            "isRequired": false,
                            "title": "date only",
                            "includeTime": false
                        }
                    ],
                    "actions": [
                        {
                            "@type": "HttpPOST",
                            "name": "send",
                            "body": "date1={{date-1.value}},date2={{date-2.value}}"
                        }
                    ]
                },
                {
                    "@type": "ViewAction",
                    "name": "Open Uri",
                    "targets": [
                        { "os": "default", "uri": "http://microsoft.com" },
                        { "os": "iOS", "uri": "http://microsoft.com" },
                        { "os": "android", "uri": "http://microsoft.com" },
                        { "os": "windows", "uri": "http://microsoft.com" }
                    ]
                },
                {
                    "@context": "http://schema.org",
                    "@type": "ViewAction",
                    "name": "View Action",
                    "target": ["http://microsoft.com"]
                }
            ]
        });
    }

    /**
     * Creates and returns the O365 actionable card with multiple sections.
     * @returns {Attachment} The O365 actionable card attachment.
     */
    O365ActionableCardMultipleSection() {
        return CardFactory.o365ConnectorCard({
            "@context": "https://schema.org/extensions",
            "@type": "MessageCard",
            "themeColor": "#E67A9E",
            "title": "This is Actionable Card Title",
            "summary": "O365 card summary",
            "sections": [
                { "title": "Section Title 1", "text": "" },
                { "title": "Section Title 2" }
            ],
            "potentialAction": [
                {
                    "@type": "ActionCard",
                    "name": "Multiple Choice",
                    "inputs": [
                        {
                            "@type": "multichoiceInput",
                            "id": "cardstype",
                            "isRequired": true,
                            "title": "Pick multiple options",
                            "choices": [
                                { "display": "Hero Card", "value": "Hero Card" },
                                { "display": "Thumbnail Card", "value": "Thumbnail Card" },
                                { "display": "O365 Connector Card", "value": "O365 Connector Card" }
                            ],
                            "style": "compact",
                            "isMultiSelect": true
                        }
                    ],
                    "actions": [
                        {
                            "@type": "HttpPOST",
                            "name": "send",
                            "body": "cardstype={{cardstype.value}}"
                        }
                    ]
                },
                {
                    "@type": "ActionCard",
                    "name": "Input Card",
                    "inputs": [
                        {
                            "@type": "textInput",
                            "id": "text-1",
                            "isRequired": false,
                            "title": "This is the title of text box",
                            "isMultiline": true
                        }
                    ],
                    "actions": [
                        {
                            "@type": "HttpPOST",
                            "name": "Send",
                            "body": "text1={{text-1.value}}"
                        }
                    ]
                },
                {
                    "@type": "ActionCard",
                    "name": "Multiple Choice",
                    "inputs": [
                        {
                            "@type": "multichoiceInput",
                            "id": "CardsTypesection1",
                            "isRequired": true,
                            "title": "This is a title of combo box",
                            "choices": [
                                { "display": "Hero Card", "value": "Hero Card" },
                                { "display": "Thumbnail Card", "value": "Thumbnail Card" },
                                { "display": "O365 Connector Card", "value": "O365 Connector Card" }
                            ],
                            "style": "compact",
                            "isMultiSelect": true
                        }
                    ],
                    "actions": [
                        {
                            "@type": "HttpPOST",
                            "name": "send",
                            "body": "CardsTypesection1={{CardsTypesection1.value}}"
                        }
                    ]
                }
            ]
        });
    }
}

exports.O365ConnectorCardActionDialog = O365ConnectorCardActionDialog;