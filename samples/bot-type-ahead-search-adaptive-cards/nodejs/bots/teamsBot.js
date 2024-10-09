// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory } = require("botbuilder");
const axios = require('axios');
const querystring = require('querystring');

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
        if (context.activity.text.toLowerCase().trim() == "staticsearch") {
          const userCard = CardFactory.adaptiveCard(this.adaptiveCardForStaticSearch());

          await context.sendActivity({ attachments: [userCard] });
        }
        else if (context.activity.text.toLowerCase().trim() == "dynamicsearch") {
          const userCard = CardFactory.adaptiveCard(this.adaptiveCardForDyanamicSearch());

          await context.sendActivity({ attachments: [userCard] });
        }
        else if (context.activity.text.toLowerCase().trim() == "dependantdropdown") {
          const userCard = CardFactory.adaptiveCard(this.adaptiveCardForDependantSearch());

          await context.sendActivity({ attachments: [userCard] });
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
    if (context._activity.name === 'application/search') {
        let dropdownCard = context._activity.value.data.choiceselect; // Get the country from the dropdown card
        let searchQuery = context._activity.value.queryText;
        
        // Fetch npm packages based on the search query
        const response = await axios.get(`http://registry.npmjs.com/-/v1/search?${querystring.stringify({ text: searchQuery, size: 8 })}`);
        let npmPackages = [];

        response.data.objects.forEach(obj => {
            const attachment = {
                "title": obj.package.name,
                "value": `${obj.package.name} - ${obj.package.description}`
            };
            npmPackages.push(attachment);
        });

        if (response.status === 200) {
            let searchResponseData;

            // Handle country-specific results
            if (dropdownCard) {
                const country = dropdownCard.toLowerCase();
                let results;

                switch (country) {
                    case 'usa':
                        results = [
                            { title: "CA", value: "CA" },
                            { title: "FL", value: "FL" },
                            { title: "TX", value: "TX" }
                        ];
                        break;
                    case 'france':
                        results = [
                            { title: "Paris", value: "Paris" },
                            { title: "Lyon", value: "Lyon" },
                            { title: "Nice", value: "Nice" }
                        ];
                        break;
                    default: // Default to India if the country is not recognized
                        results = [
                            { title: "Delhi", value: "Delhi" },
                            { title: "Mumbai", value: "Mumbai" },
                            { title: "Pune", value: "Pune" }
                        ];
                        break;
                }

                searchResponseData = {
                    status: 200,
                    body: {
                        "type": "application/vnd.microsoft.search.searchResponse",
                        "value": {
                            "results": results
                        }
                    }
                };

                return searchResponseData;
            } else {
                // If no country is specified, return npm package results
                const successResult = {
                    status: 200,
                    body: {
                        "type": "application/vnd.microsoft.search.searchResponse",
                        "value": {
                            "results": npmPackages
                        }
                    }
                };
                return successResult;
            }
        } else if (response.status === 204) {
            // No results found
            const noResultFound = {
                status: 204,
                body: {
                    "type": "application/vnd.microsoft.search.searchResponse"
                }
            };
            return noResultFound;
        } else if (response.status === 500) {
            // Internal server error
            const errorResult = {
                status: 500,
                body: {
                    "type": "application/vnd.microsoft.error",
                    "value": {
                        "code": "500",
                        "message": "Error message: Internal Server Error"
                    }
                }
            };
            return errorResult;
        }
    }

    return null;
}


  // Adaptive card for static search.
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

  // Adaptive card for dynamic search.
  adaptiveCardForDyanamicSearch = () => ({
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
                "isMultiSelect": false,
                "style": "filtered",
                "choices.data": {
                  "type": "Data.Query",
                  "dataset": "npmpackages"
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

  adaptiveCardForDependantSearch = () => ({
    "type": "AdaptiveCard",
    "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.5",
    "body": [
      {
        "size": "ExtraLarge",
        "text": "Country Picker",
        "weight": "Bolder",
        "wrap": true,
        "type": "TextBlock"
      },
      {
        "id": "choiceselect",
        "type": "Input.ChoiceSet",
        "label": "Select a country or region:",
        "choices": [
          {
            "title": "USA",
            "value": "usa"
          },
          {
            "title": "France",
            "value": "france"
          },
          {
            "title": "India",
            "value": "india"
          }
        ],
        "valueChangedAction": {
          "type": "Action.ResetInputs",
          "targetInputIds": [
            "city"
          ]
        },
        "isRequired": true,
        "errorMessage": "Please select a country or region"
      },
      {
        "style": "filtered",
        "choices.data": {
          "type": "Data.Query",
          "dataset": "cities",
          "associatedInputs": "auto"
        },
        "id": "city",
        "type": "Input.ChoiceSet",
        "label": "Select a city:",
        "placeholder": "Type to search for a city in the selected country",
        "isRequired": true,
        "errorMessage": "Please select a city"
      }
    ],
    "actions": [
      {
        "title": "Submit",
        "type": "Action.Submit"
      }
    ]
  });
}

module.exports.TeamsBot = TeamsBot;