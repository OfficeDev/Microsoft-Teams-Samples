// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory, MessageFactory } = require("botbuilder");
var chosenFlow = "";
class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();
        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let member = 0; member < membersAdded.length; member++) {
                if (membersAdded[member].id !== context.activity.recipient.id) {
                    await context.sendActivity("Hello and welcome! With this sample you can see the functionality of static and dynamic search in adaptive card");
                }
            }

            await next();
        });

        this.onMessage(async (context, next) => {

            if (context.activity.text != null) {
                if (context.activity.text.toLowerCase().trim() === "chosen flow" || context.activity.text.toLowerCase().trim() === "<at>typeahead search adaptive card</at> chosen flow") {
                    const replyActivity = MessageFactory.text(`Bot configured for ${chosenFlow} flow`);
                    await context.sendActivity(replyActivity);
                }
            }
            else if (context.activity.value != null) {
                await context.sendActivity("Selected option is: " + context.activity.value.choiceselect);
            }

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }

    async onInvokeActivity(context) {

        if (context._activity.name == "config/fetch") {
            const adaptiveCard = CardFactory.adaptiveCard(this.adaptiveCardForDynamicSearch());
            try {
                return {
                    status: 200,
                    body: {
                        config: {
                            type: 'continue',
                            value: {
                                card: adaptiveCard,
                                height: 400,
                                title: 'Task module fetch response',
                                width: 300
                            }
                        }
                    }
                }
            } catch (e) {
                console.log(e);
            }
        }

        if (context._activity.name == "config/submit") {

            const choice = context._activity.value.data.choiceselect.split(" ")[0];
            chosenFlow = choice;

            if (choice === "static_option_2") {
                const adaptiveCard = CardFactory.adaptiveCard(this.adaptiveCardForStaticSearch());

                return {
                    status: 200,
                    body: {
                        config: {
                            type: 'continue',
                            value: {
                                card: adaptiveCard,
                                height: 400,
                                title: 'Task module submit response',
                                width: 300
                            }
                        }
                    }
                }


            }
            else {

                try {
                    return {
                        status: 200,
                        body: {
                            config: {
                                type: 'message',
                                value: "end"
                            }
                        }
                    }
                }
                catch (e) {
                    console.log(e);

                }
            }

            return await super.onInvokeActivity(context);
        }

    }

    adaptiveCardForStaticSearch = () => ({
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.2",
        "type": "AdaptiveCard",
        "body": [
            {
                "text": "Please search for the IDE from static list.",
                "wrap": true,
                "type": "TextBlock"
            },
            {
                "columns": [
                    {
                        "width": "auto",
                        "items": [
                            {
                                "text": "IDE: ",
                                "wrap": true,
                                "height": "stretch",
                                "type": "TextBlock"
                            }
                        ],
                        "type": "Column"
                    }
                ],
                "type": "ColumnSet"
            },
            {
                "columns": [
                    {
                        "width": "stretch",
                        "items": [
                            {
                                "choices": [
                                    {
                                        "title": "Visual studio",
                                        "value": "visual_studio"
                                    },
                                    {
                                        "title": "IntelliJ IDEA ",
                                        "value": "intelliJ_IDEA "
                                    },
                                    {
                                        "title": "Aptana Studio 3",
                                        "value": "aptana_studio_3"
                                    },
                                    {
                                        "title": "PyCharm",
                                        "value": "pycharm"
                                    },
                                    {
                                        "title": "PhpStorm",
                                        "value": "phpstorm"
                                    },
                                    {
                                        "title": "WebStorm",
                                        "value": "webstorm"
                                    },
                                    {
                                        "title": "NetBeans",
                                        "value": "netbeans"
                                    },
                                    {
                                        "title": "Eclipse",
                                        "value": "eclipse"
                                    },
                                    {
                                        "title": "RubyMine ",
                                        "value": "rubymine "
                                    },
                                    {
                                        "title": "Visual studio code",
                                        "value": "visual_studio_code"
                                    }
                                ],
                                "style": "filtered",
                                "placeholder": "Search for a IDE",
                                "id": "choiceselect",
                                "type": "Input.ChoiceSet"
                            }
                        ],
                        "type": "Column"
                    }
                ],
                "type": "ColumnSet"
            }
        ],
        "actions": [
            {
                "type": "Action.Submit",
                "id": "submit",
                "title": "Submit"
            }
        ]
    });

    adaptiveCardForDynamicSearch = () => ({
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.2",
        "type": "AdaptiveCard",
        "body": [
            {
                "text": "Please search for npm packages using dynamic search control.",
                "wrap": true,
                "type": "TextBlock"
            },
            {
                "columns": [
                    {
                        "width": "auto",
                        "items": [
                            {
                                "text": "NPM packages search: ",
                                "wrap": true,
                                "height": "stretch",
                                "type": "TextBlock"
                            }
                        ],
                        "type": "Column"
                    }
                ],
                "type": "ColumnSet"
            },
            {
                "columns": [
                    {
                        "width": "stretch",
                        "items": [
                            {
                                "choices": [
                                    {
                                        "title": "Static Option 1",
                                        "value": "static_option_1"
                                    },
                                    {
                                        "title": "Static Option 2",
                                        "value": "static_option_2"
                                    },
                                    {
                                        "title": "Static Option 3",
                                        "value": "static_option_3"
                                    }
                                ],
                                "value": "static_option_2",
                                "isMultiSelect": false,
                                "style": "filtered",
                                "choices.data": {
                                    "type": "Data.Query",
                                    "dataset": "npmpackages",
                                    "count": 12

                                },
                                "id": "choiceselect",
                                "type": "Input.ChoiceSet"
                            }
                        ],
                        "type": "Column"
                    }
                ],
                "type": "ColumnSet"
            }
        ],
        "actions": [
            {
                "type": "Action.Submit",
                "id": "submitdynamic",
                "title": "Submit"
            }
        ]
    });
}

module.exports.TeamsBot = TeamsBot;