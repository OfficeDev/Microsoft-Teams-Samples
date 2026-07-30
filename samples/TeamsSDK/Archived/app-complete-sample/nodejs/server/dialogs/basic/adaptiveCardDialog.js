// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { CardFactory } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const ADAPTIVECARD = "AdaptiveCardDialog";

/**
 * AdaptiveCardDialog class extends ComponentDialog to handle adaptive card interactions.
 */
class AdaptiveCardDialog extends ComponentDialog {
    /**
     * Constructor for the AdaptiveCardDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(ADAPTIVECARD, [
            this.beginAdaptiveCardDialog.bind(this),
        ]));
    }

    /**
     * Begins the adaptive card dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginAdaptiveCardDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "AdaptiveCardDialog";
        if (stepContext.context._activity.value != null) {
            if (stepContext.context._activity.value.isFromAdaptiveCard) {
                const text = JSON.stringify(stepContext.context._activity.value);
                await stepContext.context.sendActivity(text);
            }
        } else {
            const reply = stepContext.context._activity;
            if (reply.attachments != null && reply.entities.length > 1) {
                reply.attachments = null;
                reply.entities.splice(0, 1);
            }

            const card = this.getAdaptiveCard();
            reply.attachments = [card];
            await stepContext.context.sendActivity(reply);
        }

        return await stepContext.endDialog();
    }

    /**
     * Creates and returns an adaptive card.
     * @returns {Attachment} The adaptive card attachment.
     */
    getAdaptiveCard() {
        const textToTriggerThisDialog = "adaptivecard";
        const adaptiveCardJson = {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.0",
            "body": [
                {
                    "type": "Container",
                    "items": [
                        {
                            "type": "TextBlock",
                            "size": "large",
                            "weight": "bolder",
                            "text": "Adaptive Card!",
                            "horizontalAlignment": "left",
                            "wrap": false,
                            "speak": "<s>Adaptive card!</s>",
                        },
                        {
                            "type": "FactSet",
                            "facts": [
                                { "title": "Board:", "value": "Adaptive Card" },
                                { "title": "List:", "value": "Backlog" },
                                { "title": "Assigned to:", "value": "Matt Hidinger" },
                                { "title": "Due date:", "value": "Not set" },
                            ],
                        },
                        {
                            "type": "ImageSet",
                            "images": [
                                { "type": "Image", "url": "http://contososcubabot.azurewebsites.net/assets/steak.jpg" },
                                { "type": "Image", "url": "http://contososcubabot.azurewebsites.net/assets/chicken.jpg" },
                                { "type": "Image", "url": "http://contososcubabot.azurewebsites.net/assets/tofu.jpg" },
                            ],
                            "imageSize": "medium",
                        },
                        {
                            "type": "TextBlock",
                            "text": "'**Matt H. said** \"I'm compelled to give this place 5 stars due to the number of times I've chosen to eat here this past year!',",
                            "horizontalAlignment": "left",
                            "wrap": true,
                        },
                        {
                            "type": "TextBlock",
                            "text": "Place your text here:",
                            "horizontalAlignment": "left",
                            "wrap": false,
                        },
                        {
                            "type": "Input.Text",
                            "placeholder": "Text Input",
                            "style": "text",
                            "id": "textInputId",
                        },
                        {
                            "type": "TextBlock",
                            "text": "Please select Date here?",
                            "horizontalAlignment": "left",
                            "wrap": false,
                        },
                        {
                            "type": "Input.Date",
                            "id": "dateInput",
                        },
                        {
                            "type": "TextBlock",
                            "text": "Please enter time here?",
                            "horizontalAlignment": "left",
                            "wrap": false,
                        },
                        {
                            "type": "Input.Time",
                            "id": "timeInput",
                        },
                        {
                            "type": "TextBlock",
                            "text": "Please select your choice here? (Compact Dropdown)",
                            "horizontalAlignment": "left",
                            "wrap": false,
                        },
                        {
                            "type": "Input.ChoiceSet",
                            "value": "1",
                            "style": "compact",
                            "choices": [
                                { "title": "Red", "value": "1" },
                                { "title": "Green", "value": "2" },
                                { "title": "Blue", "value": "3" },
                                { "title": "White", "value": "4" },
                            ],
                            "id": "choiceSetCompact",
                        },
                        {
                            "type": "TextBlock",
                            "text": "Please select your choice here? (Expanded Dropdown)",
                            "horizontalAlignment": "left",
                            "wrap": false,
                        },
                        {
                            "type": "Input.ChoiceSet",
                            "value": "1",
                            "style": "expanded",
                            "choices": [
                                { "title": "Red", "value": "1" },
                                { "title": "Green", "value": "2" },
                                { "title": "Blue", "value": "3" },
                                { "title": "White", "value": "4" },
                            ],
                            "id": "choiceSetExpandedRequired",
                            "isRequired": true,
                        },
                        {
                            "type": "TextBlock",
                            "text": "Please select multiple items here? (Multiselect Dropdown)",
                            "horizontalAlignment": "left",
                            "wrap": false,
                        },
                        {
                            "type": "Input.ChoiceSet",
                            "value": "1,2",
                            "style": "expanded",
                            "isMultiSelect": true,
                            "choices": [
                                { "title": "Red", "value": "1" },
                                { "title": "Green", "value": "2" },
                                { "title": "Blue", "value": "3" },
                                { "title": "White", "value": "4" },
                            ],
                            "id": "choiceSetExpanded",
                        },
                        {
                            "type": "ColumnSet",
                            "columns": [
                                {
                                    "type": "Column",
                                    "size": "Auto",
                                    "items": [
                                        {
                                            "type": "Image",
                                            "size": "medium",
                                            "style": "person",
                                            "url": "https://placeholdit.imgix.net/~text?txtsize=65&txt=Adaptive+Cards&w=300&h=300",
                                        },
                                    ],
                                },
                                {
                                    "type": "Column",
                                    "size": "Stretch",
                                    "items": [
                                        {
                                            "type": "TextBlock",
                                            "weight": "bolder",
                                            "isSubtle": true,
                                            "text": "Hello!",
                                            "horizontalAlignment": "left",
                                            "wrap": false,
                                        },
                                        {
                                            "type": "TextBlock",
                                            "text": "Are you looking for a Tab or Bot?",
                                            "horizontalAlignment": "left",
                                            "wrap": true,
                                        },
                                    ],
                                },
                            ],
                        },
                        {
                            "type": "Input.Toggle",
                            "title": "I accept the terms and conditions (True/False)",
                            "valueOn": "true",
                            "valueOff": "false",
                            "id": "AcceptsTerms",
                        },
                    ],
                },
            ],
            "actions": [
                {
                    "type": "Action.Submit",
                    "title": "Submit",
                    "speak": "<s>Search</s>",
                    "data": {
                        "isFromAdaptiveCard": true,
                        "messageText": textToTriggerThisDialog,
                    },
                },
                {
                    "type": "Action.ShowCard",
                    "card": {
                        "type": "AdaptiveCard",
                        "version": "1.0",
                        "body": [
                            {
                                "type": "Container",
                                "items": [
                                    {
                                        "type": "Input.Text",
                                        "placeholder": "text here",
                                        "style": "text",
                                        "id": "Text",
                                    },
                                ],
                            },
                        ],
                        "actions": [
                            {
                                "type": "Action.Submit",
                                "title": "Submit",
                                "speak": "<s>Search</s>",
                                "data": {
                                    "isFromAdaptiveCard": true,
                                    "messageText": textToTriggerThisDialog,
                                },
                            },
                        ],
                    },
                },
                {
                    "type": "Action.OpenUrl",
                    "url": "http://adaptivecards.io/explorer/Action.OpenUrl.html",
                    "title": "Open Url",
                },
            ],
        };

        return CardFactory.adaptiveCard(adaptiveCardJson);
    }
}

exports.AdaptiveCardDialog = AdaptiveCardDialog;