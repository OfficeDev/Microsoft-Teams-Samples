// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory } = require("botbuilder");
const axios = require('axios');
const querystring = require('querystring');

class TeamsBot extends TeamsActivityHandler {
  constructor() {
    super();
    this.onMembersAdded(this.handleMembersAdded.bind(this));
    this.onMessage(this.handleMessage.bind(this));
  }

  /**
   * Handles members added to the conversation.
   * @param {TurnContext} context - The context object for the turn.
   * @param {Function} next - The next middleware or handler to run.
   */
  async handleMembersAdded(context, next) {
    const membersAdded = context.activity.membersAdded;
    for (const member of membersAdded) {
      if (member.id !== context.activity.recipient.id) {
        await context.sendActivity("Hello and welcome! With this sample you can see the functionality of static and dynamic search in adaptive card");
      }
    }
    await next();
  }

  /**
   * Handles incoming messages.
   * @param {TurnContext} context - The context object for the turn.
   * @param {Function} next - The next middleware or handler to run.
   */
  async handleMessage(context, next) {
    const text = context.activity.text?.toLowerCase().trim();
    const value = context.activity.value;

    if (text) {
      switch (text) {
        case "staticsearch":
          await context.sendActivity({ attachments: [CardFactory.adaptiveCard(this.getStaticSearchCard())] });
          break;
        case "dynamicsearch":
          await context.sendActivity({ attachments: [CardFactory.adaptiveCard(this.getDynamicSearchCard())] });
          break;
        case "dependantdropdown":
          await context.sendActivity({ attachments: [CardFactory.adaptiveCard(this.getDependantSearchCard())] });
          break;
        default:
          await context.sendActivity("Unknown command. Please use 'staticsearch', 'dynamicsearch', or 'dependantdropdown'.");
      }
    } else if (value) {
      await context.sendActivity(`Selected option is: ${value.choiceselect}`);
    }

    await next();
  }

  /**
   * Handles invoke activities.
   * @param {TurnContext} context - The context object for the turn.
   * @returns {Promise<Object>} The response object.
   */
  async onInvokeActivity(context) {
    if (context._activity.name === 'application/search') {
      const dropdownCard = context._activity.value.data?.choiceselect;
      const searchQuery = context._activity.value.queryText;

      try {
        const response = await axios.get(`http://registry.npmjs.com/-/v1/search?${querystring.stringify({ text: searchQuery, size: 8 })}`);

        if (response.status === 200 && response.data.objects.length > 0) {
          const npmPackages = response.data.objects.map(obj => ({
            title: obj.package.name,
            value: `${obj.package.name} - ${obj.package.description || "No description available"}`
          }));

          if (dropdownCard) {
            return this.getCountrySpecificResults(dropdownCard.toLowerCase());
          } else {
            return this.getSuccessResult(npmPackages);
          }
        } else {
          // No results found
          await context.sendActivity("Packages not found for your search.");
          return this.getNoResultFound();
        }
      } catch (error) {
        // Send message in Terminal when no packages are found
        console.error("We couldn’t find any packages matching your search. Try searching again with different keywords:");


       // Send message in Teams when no packages are found
        await context.sendActivity("We couldn’t find any packages matching your search. This might be due to an incorrect query. Try refining your search.");

        // Safe invoke response (still required even on failure)
        return {
          status: 200,
          body: {
            type: "application/vnd.microsoft.error",
            value: {
              code: "fetch_error",
              message: "Handled gracefully: fetch failed"
            }
          }
        };
      }
    }

    return {
      status: 200,
      body: {}
    };
  }



  /**
   * Returns the adaptive card for static search.
   * @returns {Object} The adaptive card JSON.
   */
  getStaticSearchCard() {
    return {
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
                    { "title": "Visual studio", "value": "visual_studio" },
                    { "title": "IntelliJ IDEA", "value": "intelliJ_IDEA" },
                    { "title": "Aptana Studio 3", "value": "aptana_studio_3" },
                    { "title": "PyCharm", "value": "pycharm" },
                    { "title": "PhpStorm", "value": "phpstorm" },
                    { "title": "WebStorm", "value": "webstorm" },
                    { "title": "NetBeans", "value": "netbeans" },
                    { "title": "Eclipse", "value": "eclipse" },
                    { "title": "RubyMine", "value": "rubymine" },
                    { "title": "Visual studio code", "value": "visual_studio_code" }
                  ],
                  "style": "filtered",
                  "placeholder": "Search for an IDE",
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
    };
  }

  /**
   * Returns the adaptive card for dynamic search.
   * @returns {Object} The adaptive card JSON.
   */
  getDynamicSearchCard() {
    return {
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
                    { "title": "Static Option 1", "value": "static_option_1" },
                    { "title": "Static Option 2", "value": "static_option_2" },
                    { "title": "Static Option 3", "value": "static_option_3" }
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
    };
  }

  /**
   * Returns the adaptive card for dependant search.
   * @returns {Object} The adaptive card JSON.
   */
  getDependantSearchCard() {
    return {
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
            { "title": "USA", "value": "usa" },
            { "title": "France", "value": "france" },
            { "title": "India", "value": "india" }
          ],
          "valueChangedAction": {
            "type": "Action.ResetInputs",
            "targetInputIds": ["city"]
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
    };
  }

  /**
   * Returns the success result for npm package search.
   * @param {Array} npmPackages - The list of npm packages.
   * @returns {Object} The success result object.
   */
  getSuccessResult(npmPackages) {
    return {
      status: 200,
      body: {
        "type": "application/vnd.microsoft.search.searchResponse",
        "value": {
          "results": npmPackages
        }
      }
    };
  }

  /**
   * Returns the country-specific results.
   * @param {string} country - The selected country.
   * @returns {Object} The country-specific results object.
   */
  getCountrySpecificResults(country) {
    const results = {
      usa: [
        { title: "CA", value: "CA" },
        { title: "FL", value: "FL" },
        { title: "TX", value: "TX" }
      ],
      france: [
        { title: "Paris", value: "Paris" },
        { title: "Lyon", value: "Lyon" },
        { title: "Nice", value: "Nice" }
      ],
      india: [
        { title: "Delhi", value: "Delhi" },
        { title: "Mumbai", value: "Mumbai" },
        { title: "Pune", value: "Pune" }
      ]
    };

    return {
      status: 200,
      body: {
        "type": "application/vnd.microsoft.search.searchResponse",
        "value": {
          "results": results[country] || results.india
        }
      }
    };
  }

  /**
   * Returns the no result found response.
   * @returns {Object} The no result found response object.
   */
  getNoResultFound() {
    return {
      status: 204,
      body: {
        "type": "application/vnd.microsoft.search.searchResponse"
      }
    };
  }

  /**
   * Returns the error result response.
   * @returns {Object} The error result response object.
   */
  getErrorResult() {
    return {
      status: 500,
      body: {
        "type": "application/vnd.microsoft.error",
        "value": {
          "code": "500",
          "message": "Error message: Internal Server Error"
        }
      }
    };
  }
}

module.exports.TeamsBot = TeamsBot;