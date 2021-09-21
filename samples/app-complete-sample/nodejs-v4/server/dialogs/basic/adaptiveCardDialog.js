// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { CardFactory } = require('botbuilder');
const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const ADAPTIVECARD = "AdaptiveCardDialog"

class AdaptiveCardDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(ADAPTIVECARD, [
            this.beginAdaptiveCardDialog.bind(this),
        ]));
    }

    async beginAdaptiveCardDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "AdaptiveCardDialog";
        if(stepContext.context._activity.value!=null){
            if(stepContext.context._activity.value.isFromAdaptiveCard){
                var text = JSON.stringify(stepContext.context._activity.value);
                await stepContext.context.sendActivity(text);
            }          
        }
        else{
            var reply = stepContext.context._activity;
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

    getAdaptiveCard = () => {
        let textToTriggerThisDialog = "adaptivecard";
        let adaptiveCardJson = {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.0",
            "body": [
                {
                    "type": "Container",
                    "items":
                        [
                            // TextBlock Item allows for the inclusion of text, with various font sizes, weight and color,
                            {
                                "type": "TextBlock",
                                "size": "large", // set the size of text e.g. Extra Large, Large, Medium, Normal, Small
                                "weight": "bolder", // set the weight of text e.g. Bolder, Light, Normal
                                "color": null,
                                "isSubtle": false,
                                "text": "Adaptive Card!",
                                "horizontalAlignment": "left",
                                "wrap": false,
                                "maxLines": 0,
                                "speak": "<s>Adaptive card!</s>",
                                "separation": null,
                            },
                            // Adaptive FactSet item makes it simple to display a series of facts (e.g. name/value pairs) in a tabular form
                            {
                                "type": "FactSet",
                                "facts": [
                                    // Describes a fact in a Adaptive FactSet as a key/value pair
                                    {
                                        "title": "Board:",
                                        "value": "Adaptive Card",
                                    },
                                    {
                                        "title": "List:",
                                        "value": "Backlog",
                                    },
                                    {
                                        "title": "Assigned to:",
                                        "value": "Matt Hidinger",
                                    },
                                    {
                                        "title": "Due date:",
                                        "value": "Not set",
                                    },
                                ],
                                "separation": null,
                            },
                            // ImageSet allows for the inclusion of a collection images like a photogallery
                            {
                                "type": "ImageSet",
                                "images": [
                                    // Image Item allows for the inclusion of images
                                    {
                                        "type": "Image",
                                        "size": null,
                                        "style": null,
                                        "url": "http://contososcubabot.azurewebsites.net/assets/steak.jpg",
                                        "horizontalAlignment": "left",
                                        "separation": null,
                                    },
                                    {
                                        "type": "Image",
                                        "size": null,
                                        "style": null,
                                        "url": "http://contososcubabot.azurewebsites.net/assets/chicken.jpg",
                                        "horizontalAlignment": "left",
                                        "separation": null,
                                    },
                                    {
                                        "type": "Image",
                                        "size": null,
                                        "style": null,
                                        "url": "http://contososcubabot.azurewebsites.net/assets/tofu.jpg",
                                        "horizontalAlignment": "left",
                                        "separation": null,
                                    },
                                ],
                                "imageSize": "medium",
                                "separation": null,
                            },
                            // wrap the text in textblock
                            {
                                "type": "TextBlock",
                                "size": null,
                                "weight": null,
                                "color": null,
                                "isSubtle": false,
                                // markdown example for bold text
                                "text": "'**Matt H. said** \"I'm compelled to give this place 5 stars due to the number of times I've chosen to eat here this past year!',",
                                "horizontalAlignment": "left",
                                "wrap": true, // True if text is allowed to wrap
                                "maxLines": 0,
                                "separation": null,
                            },
                            {
                                "type": "TextBlock",
                                "size": null,
                                "weight": null,
                                "color": null,
                                "isSubtle": false,
                                "text": "Place your text here:",
                                "horizontalAlignment": "left",
                                "wrap": false,
                                "maxLines": 0,
                                "separation": null,
                            },
                            {
                                "type": "Input.Text", // set the type of input box e.g Text, Tel, Email, Url
                                "placeholder": "Text Input",
                                "style": "text",
                                "isMultiline": false,
                                "maxLength": 0,
                                "id": "textInputId",
                                "isRequired": false,
                                "speak": "<s>Please enter your text here</s>",
                                "separation": null,
                            },
                            {
                                "type": "TextBlock",
                                "size": null,
                                "weight": null,
                                "color": null,
                                "isSubtle": false,
                                "text": "Please select Date here?",
                                "horizontalAlignment": "left",
                                "wrap": false,
                                "maxLines": 0,
                                "separation": null,
                            },
                            // date input collects Date from the user
                            {
                                "type": "Input.Date",
                                "id": "dateInput",
                                "isRequired": false,
                                "speak": "<s>Please select Date here?</s>",
                                "separation": null,
                            },
                            {
                                "type": "TextBlock",
                                "size": null,
                                "weight": null,
                                "color": null,
                                "isSubtle": false,
                                "text": "Please enter time here?",
                                "horizontalAlignment": "left",
                                "wrap": false,
                                "maxLines": 0,
                                "separation": null,
                            },
                            // time input collects time from the user
                            {
                                "type": "Input.Time",
                                "id": "timeInput",
                                "isRequired": false,
                                "separation": null,
                            },
                            {
                                "type": "TextBlock",
                                "size": null,
                                "weight": null,
                                "color": null,
                                "isSubtle": false,
                                "text": "Please select your choice here? (Compact Dropdown)",
                                "horizontalAlignment": "left",
                                "wrap": false,
                                "maxLines": 0,
                                "separation": null,
                            },
                            // Shows an array of Choice objects
                            {
                                "type": "Input.ChoiceSet",
                                "value": 1,
                                "style": "compact", // set the style of Choice set to compact
                                "isMultiSelect": false,
                                "choices": [
                                    // describes a choice input. the value should be a simple string without a ","
                                    {
                                        "title": "Red",
                                        "value": 1, // do not use a “,” in the value, since MultiSelect ChoiceSet returns a comma-delimited string of choice values
                                        "isSelected": false,
                                    },
                                    {
                                        "title": "Green",
                                        "value": 2,
                                        "isSelected": false,
                                    },
                                    {
                                        "title": "Blue",
                                        "value": 3,
                                        "isSelected": false,
                                    },
                                    {
                                        "title": "White",
                                        "value": 4,
                                        "isSelected": false,
                                    },
                                ],
                                "id": "choiceSetCompact",
                                "isRequired": false,
                                "separation": null,
                            },
                            {
                                "type": "TextBlock",
                                "size": null,
                                "weight": null,
                                "color": null,
                                "isSubtle": false,
                                "text": "Please select your choice here? (Expanded Dropdown)",
                                "horizontalAlignment": "left",
                                "wrap": false,
                                "maxLines": 0,
                                "separation": null,
                            },
                            // Shows an array of Choice objects
                            {
                                "type": "Input.ChoiceSet",
                                "value": 1, // please set default value here
                                "style": "expanded", // set the style of Choice set to expanded
                                "isMultiSelect": false,
                                "choices": [
                                    {
                                        "title": "Red",
                                        "value": 1,
                                        "isSelected": false,
                                    },
                                    {
                                        "title": "Green",
                                        "value": 2,
                                        "isSelected": false,
                                    },
                                    {
                                        "title": "Blue",
                                        "value": 3,
                                        "isSelected": false,
                                    },
                                    {
                                        "title": "White",
                                        "value": 4,
                                        "isSelected": false,
                                    },
                                ],
                                "id": "choiceSetExpandedRequired",
                                "isRequired": true,
                                "separation": null,
                            },
                            {
                                "type": "TextBlock",
                                "size": null,
                                "weight": null,
                                "color": null,
                                "isSubtle": false,
                                "text": "Please select multiple items here? (Multiselect Dropdown)",
                                "horizontalAlignment": "left",
                                "wrap": false,
                                "maxLines": 0,
                                "separation": null,
                            },
                            // Shows an array of Choice objects (Multichoice)
                            {
                                "type": "Input.ChoiceSet",
                                "value": "1,2", // The initial choice (or set of choices) that should be selected. For multi-select, specifcy a comma-separated string of values
                                "style": "expanded", // set the style of Choice set to expanded
                                "isMultiSelect": true, // allow multiple choices to be selected
                                "choices": [
                                    {
                                        "title": "Red",
                                        "value": 1,
                                        "isSelected": false,
                                    },
                                    {
                                        "title": "Green",
                                        "value": 2,
                                        "isSelected": false,
                                    },
                                    {
                                        "title": "Blue",
                                        "value": 3,
                                        "isSelected": false,
                                    },
                                    {
                                        "title": "White",
                                        "value": 4,
                                        "isSelected": false,
                                    },
                                ],
                                "id": "choiceSetExpanded",
                                "isRequired": false,
                                "separation": null,
                            },
                            // column set divides a region into Column's allowing elements to sit side-by-side
                            {
                                "type": "ColumnSet",
                                "columns": [
                                    {
                                        // defines a container that is part of a column set
                                        "type": "Column",
                                        "size": "Auto",
                                        "items": [
                                            {
                                                "type": "Image",
                                                "size": "medium",
                                                "style": "person",
                                                "url": "https://placeholdit.imgix.net/~text?txtsize=65&txt=Adaptive+Cards&w=300&h=300",
                                                "horizontalAlignment": "left",
                                                "separation": null,
                                            },
                                        ],
                                        "style": null,
                                        "separation": null,
                                    },
                                    {
                                        "type": "Column",
                                        "size": "Stretch",
                                        "items": [
                                            {
                                                "type": "TextBlock",
                                                "size": null,
                                                "weight": "bolder",
                                                "color": null,
                                                "isSubtle": true,
                                                "text": "Hello!",
                                                "horizontalAlignment": "left",
                                                "wrap": false,
                                                "maxLines": 0,
                                                "separation": null,
                                            },
                                            {
                                                "type": "TextBlock",
                                                "size": null,
                                                "weight": null,
                                                "color": null,
                                                "isSubtle": false,
                                                "text": "Are you looking for a Tab or Bot?",
                                                "horizontalAlignment": "left",
                                                "wrap": true,
                                                "maxLines": 0,
                                                "separation": null,
                                            },
                                        ],
                                        "style": null,
                                        "separation": null,
                                    },
                                ],
                            },
                            // input toggle collects a true/false response from the user
                            {
                                "type": "Input.Toggle",
                                "title": "I accept the terms and conditions (True/False)",
                                "valueOn": "true", // the value when toggle is on (default: true)
                                "valueOff": "false", // the value when toggle is off (default: false)
                                "id": "AcceptsTerms",
                                "isRequired": false,
                                "separation": null,
                            },
                        ],
                },
            ],
            "actions": [
                // submit action gathers up input fields, merges with optional data field and generates event to client asking for data to be submitted
                {
                    "type": "Action.Submit",
                    "title": "Submit",
                    "speak": "<s>Search</s>",
                    "data": {
                        "isFromAdaptiveCard": true,
                        "messageText": textToTriggerThisDialog,
                    },
                },
                // show action defines an inline AdaptiveCard which is shown to the user when it is clicked
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
                                        "isMultiline": false,
                                        "maxLength": 0,
                                        "id": "Text",
                                        "isRequired": false,
                                        "speak": "<s>Please enter your text here?</s>",
                                        "separation": null,
                                    },
                                ],
                                "style": null,
                                "separation": null,
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
                // open url show the given url, either by launching it to an external web browser
                {
                    "type": "Action.OpenUrl",
                    "url": "http://adaptivecards.io/explorer/Action.OpenUrl.html",
                    "title": "Open Url",
                },
            ],
        };

        var card = CardFactory.adaptiveCard(adaptiveCardJson);
        return card;
    }
}

exports.AdaptiveCardDialog = AdaptiveCardDialog;