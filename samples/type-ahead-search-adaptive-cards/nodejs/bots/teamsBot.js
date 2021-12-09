// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory, ActivityHandler } = require("botbuilder");
const request = require('request-promise')
const searchApiUrlFormat = "https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch=[keyword]&srlimit=[limit]&sroffset=[offset]&format=json";

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
      }
      else if (context.activity.value != null) {
        await context.sendActivity("Select IDE is: " + context.activity.value.choiceIDESingle);
      }

      // By calling next() you ensure that the next BotHandler is run.
      await next();
    });
  }

  async onInvokeActivity(context) {

    if (context._activity.name == 'application/search') {
      let searchApiUrl = searchApiUrlFormat.replace("[keyword]", context._activity.value.queryText);
      searchApiUrl = searchApiUrl.replace("[limit]", context._activity.value.queryOptions.top + "");
      searchApiUrl = searchApiUrl.replace("[offset]", context._activity.value.queryOptions.skip + "");
      searchApiUrl = encodeURI(searchApiUrl);
      let promisesOfCardsAsAttachments = [];

      //   var result = await new Promise(function (resolve, reject) {
      //        request(searchApiUrl, (error, res, body) => {
      //          let wikiResults = JSON.parse(body).query.search;
      //        wikiResults.forEach((wikiResult) => {

      //        const attachment = { 
      //          "title": wikiResult.title,
      //          "value": wikiResult.snippet
      //        };
      //        promisesOfCardsAsAttachments.push(attachment);

      //     });
      //      let response = {
      //       status : 200,
      //       body:{"data":promisesOfCardsAsAttachments}
      //      }
      //       resolve(response);
      //      });
      //  });

      var data = {
        "choices": [
          {
            "title": "hello",
            "value": "hey"
          },
          {
            "title": "hello1",
            "value": "hey"
          }, {
            "title": "hello2",
            "value": "hey"
          }, {
            "title": "hello3",
            "value": "hey"
          }, {
            "title": "hello4",
            "value": "hey"
          }, {
            "title": "hello5",
            "value": "hey"
          }, {
            "title": "hello6",
            "value": "hey"
          }, {
            "title": "hello7",
            "value": "hey"
          }, {
            "title": "hello8",
            "value": "hey"
          }, {
            "title": "hello9",
            "value": "hey"
          }, {
            "title": "hello10",
            "value": "hey"
          }, {
            "title": "hello11",
            "value": "hey"
          }]
      }
      var result = ActivityHandler.createInvokeResponse(JSON.stringify(data));

      return result;
    }

    return null;
  }

  adaptiveCardForStaticSearch = () => ({
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
                "id": "choiceIDESingle",
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
    ],
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.2"
  });

  adaptiveCardForDyanamicSearch = () => ({
    "type": "AdaptiveCard",
    "body": [
      {
        "text": "Please fill out the below form to send a game purchase request.",
        "wrap": true,
        "type": "TextBlock"
      },
      {
        "columns": [
          {
            "width": "auto",
            "items": [
              {
                "text": "Multi-Game: ",
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
                "isMultiSelect": true,
                "style": "filtered",
                "choices.data": {
                  "type": "Data.Query",
                  "dataset": "graph.microsoft.com/me/joinedTeams"
                },
                "id": "choiceGameMulti",
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
    ],
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.2"
  });
}

module.exports.TeamsBot = TeamsBot;