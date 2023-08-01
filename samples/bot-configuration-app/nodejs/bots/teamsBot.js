// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory, ActivityHandler,MessageFactory } = require("botbuilder");
const axios = require('axios');
const querystring = require('querystring');
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
      console.log("context type",context.activity.conversation.conversationType)
      console.log("context value",context.activity.text.toLowerCase().trim())
      if (context.activity.text != null) {
        if (context.activity.text.toLowerCase().trim() === "staticsearch" || context.activity.text.toLowerCase().trim() === "<at>typeahead search adaptive card</at> staticsearch") {
          const userCard = CardFactory.adaptiveCard(this.adaptiveCardForStaticSearch());
          console.log("reaching here!")
          await context.sendActivity({ attachments: [userCard] });
        }
        else if (context.activity.text.toLowerCase().trim() === "table"){
          const card = CardFactory.adaptiveCard(this.adaptiveCardTable());
          await context.sendActivity({attachments:[card]});
        }
        else if (context.activity.text.toLowerCase().trim() === "dynamicsearch" || context.activity.text.toLowerCase().trim() === "<at>typeahead search adaptive card</at> dynamicsearch") {
          const userCard = CardFactory.adaptiveCard(this.adaptiveCardForDynamicSearch());

          await context.sendActivity({ attachments: [userCard] });
        }
        else if (context.activity.text.toLowerCase().trim() === "showcardrtl"){
          const card = CardFactory.adaptiveCard(this.adaptiveCardShowCardRtl());
          await context.sendActivity({attachments:[card]});
        }
        else if (context.activity.text.toLowerCase().trim() === "cardrtl"){
          const card = CardFactory.adaptiveCard(this.adaptiveCardRtl());
          await context.sendActivity({attachments:[card]});
        }
        else if (context.activity.text.toLowerCase().trim() === "carousel" || context.activity.text.toLowerCase().trim() === "<at>typeahead search adaptive card</at> carousel"){
          const card1 = CardFactory.adaptiveCard(this.adaptiveCardCarousel());
          const card2 = CardFactory.adaptiveCard(this.adaptiveCardCarousel());
          await context.sendActivity({attachments:[card1,card2],attachmentLayout:"carousel"});
        }
        else if (context.activity.text.toLowerCase().trim() === "mention" || context.activity.text.toLowerCase().trim() === "<at>typeahead search adaptive card</at> mention"){
          const mention = {
            mentioned: 
            {id:"anapandey@microsoft.com",name:"Anamika Pandey"},
            text: `<at>Anamike Pandey</at>`,
            type: "mention"
        };
        console.log("mention",mention)
        // Returns a simple text message.
        const replyActivity = MessageFactory.text(`Hello ${mention.text}`);
        replyActivity.entities = [mention];
    
        // Sends a message activity to the sender of the incoming activity.
        await context.sendActivity(replyActivity);
        }
        else if (context.activity.text.toLowerCase().trim() === "url msg"){
        // Returns a simple text message.
        const replyActivity = MessageFactory.text(`[tel](tel:1-908-739-7870)`);
        
    
        // Sends a message activity to the sender of the incoming activity.
        await context.sendActivity(replyActivity);
        }
        else if (context.activity.text.toLowerCase().trim() === "chosen flow" || context.activity.text.toLowerCase().trim() === "<at>typeahead search adaptive card</at> chosen flow"){
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

  async handleTeamsMessagingExtensionFetchTask(context,action) {
    console.log("fetch invoke", context.activity)
    const adaptiveCard = CardFactory.adaptiveCard(this.adaptiveCardForDynamicSearch());
    switch (action.commandId) {
      case "auth":
      case 'testCommand':
      default:
        // try {
        //   return {
        //     composeExtension: {
        //       type: 'auth',
        //       suggestedActions: {
        //           actions: [
        //               {
        //                   type: 'openUrl',
        //                   value: "www.google.com",
        //                   title: 'Bot Service OAuth'
        //               },
        //           ],
        //       }
        //   }
        //           }}
        // catch (e) {
        //   console.log(e);
        //   console.log("auth errroor");
        // }

        try {
          
            return {
              task: {
                type: 'continue',
                value: {
                    card: adaptiveCard,
                    height: 400,
                    title: 'Task module fetch',
                    width: 300
                }
            }
            };
        } catch (e) {
           console.log(e);
           console.log("errroor");
        }
    }
  }

  async handleTeamsMessagingExtensionQuery(context, query) {
    console.log("search", context.activity)
    return {
      composeExtension: {
        type: 'auth',
        suggestedActions: {
            actions: [
                {
                    type: 'openUrl',
                    value: "www.google.com",
                    title: 'Bot Service OAuth'
                },
            ],
        }
    }
            }
  }

  async handleTeamsMessagingExtensionSubmitAction(context,action){
    console.log("handling submit",context.activity)
    const adaptiveCard = CardFactory.adaptiveCard(this.adaptiveCardForDynamicSearch());
    return {
      composeExtension: {
          type: 'result',
          attachmentLayout: 'list',
          attachments: [
              adaptiveCard
          ]
      }
  };
  }

  async onInvokeActivity(context) {
    
    if (context._activity.name == 'application/search') {
      console.log("search acvtity",context._activity)
      let searchQuery = context._activity.value.queryText;
      let size = context._activity.value.queryOptions.top;
      const response = await axios.get(`http://registry.npmjs.com/-/v1/search?${querystring.stringify({ text: searchQuery, size: size })}`);
      let npmPackages = [];
      console.log("response",response)
      response.data.objects.forEach(obj => {
        const attatchment = {
          "title": obj.package.name,
          "value": `${obj.package.name} - ${obj.package.description}`
        };

        npmPackages.push(attatchment);
      });
      console.log("context",context._activity);
      if (response.status == 200){
        var successResult = {
          status: 200,
          body:{
            "type": "application/vnd.microsoft.search.searchResponse",
            "value": {
              "results": npmPackages
          }
        }
      }
  
        return successResult;
      }
      else if(response.status == 204){
        var noResultFound = {
          status: 204,
          body:{
            "type": "application/vnd.microsoft.search.searchResponse" 
          }
        }
  
        return noResultFound;
      }
      else if(response.status == 500){
        var errorResult = {
          status: 500,
          body:{
            "type": "application/vnd.microsoft.error",
            "value": {
              "code": "500", 
              "message": "error message: internal Server Error" 
          }
        }
      }

        return errorResult;
      }

    }

    if (context._activity.name == "config/fetch"){
      console.log("appears in config fetch")
      const adaptiveCard = CardFactory.adaptiveCard(this.adaptiveCardForDynamicSearch());
      try {  
        return {
          status: 200,
          body:{
            config: {
              type: 'continue',
              value: {
                card:adaptiveCard,
                // url:"https://www.microsoft.com",
                height: 400,
                title: 'Task module fetch response',
                 width: 300
              }}
        }}     
 
//     return {
//       status: 200,
//       body:{
//       config: {
//         type: 'auth',
//         suggestedActions: {
//           actions: [
//               {
//                   type: 'openUrl',
//                   value: "www.teams.microsoft.com",
//                   title: 'Bot Service OAuth'
//               },
//           ],
//       }
//     }
//     }
// }
  }catch (e) {
       console.log(e);
       console.log("errroor");
    }      
    }

    if (context._activity.name == "config/submit"){
      
      const choice = context._activity.value.data.choiceselect.split(" ")[0];
      console.log("appears in config submit", choice)
      chosenFlow = choice;
      
       if(choice==="static_option_2"){
        const adaptiveCard = CardFactory.adaptiveCard(this.adaptiveCardForStaticSearch());
                
          return {
            status: 200,
            body:{
              config: {
                type: 'continue',
                value: {
                  card: adaptiveCard,
                  height: 400,
                  title: 'Task module submit response',
                   width: 300
                }}
          }}
          
       
       }
      else {
        
        try {         
          return {
            status: 200,
            body:{
              config: {
                type: 'message',
                value: "end"}
          }}
          }
       catch (e) {
         console.log(e);
         console.log("errroor");
         
      }
      }
          return await super.onInvokeActivity(context);
  }}

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
                "isMultiSelect": true,
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
  adaptiveCardTable = ()=>(
  {
      "type": "AdaptiveCard",
      "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
      "version": "1.5",
      "body": [
          {
              "type": "TextBlock",
              "text": "Table with gridlines",
              "wrap": true,
              "style": "heading"
          },
          {
              "type": "Table",
              
              "columns": [
                  {
                      "width": 1
                  },
                  {
                      "width": 1
                  }
              ],
              "rows": [
                  {
                      "type": "TableRow",
                      "cells": [
                          {
                              "type": "TableCell",
                              "items": [
                                  {
                                      "type": "TextBlock",
                                      "text": "showGridLines value",
                                      "wrap": true
                                  }
                              ],
                              "style": "accent"
                          },
                          {
                              "type": "TableCell",
                              "items": [
                                  {
                                      "type": "TextBlock",
                                      "text": "Behavior",
                                      "wrap": true
                                  }
                              ],
                              "style": "accent"
                          }
                      ]
                  },
                  {
                      "type": "TableRow",
                      "cells": [
                          {
                              "type": "TableCell",
                              "items": [
                                  {
                                      "type": "TextBlock",
                                      "text": "true",
                                      "wrap": true
                                  }
                              ],
                              "style": "accent"
                          },
                          {
                              "type": "TableCell",
                              "items": [
                                  {
                                      "type": "TextBlock",
                                      "text": "Cells are separated by 1px width gridlines",
                                      "wrap": true
                                  }
                              ],
                              "style": "accent"
                          }
                      ]
                  },
                  {
                      "type": "TableRow",
                      "cells": [
                          {
                              "type": "TableCell",
                              "items": [
                                  {
                                      "type": "TextBlock",
                                      "text": "false",
                                      "wrap": true
                                  }
                              ],
                              "style": "accent"
                          },
                          {
                              "type": "TableCell",
                              "items": [
                                  {
                                      "type": "TextBlock",
                                      "text": "Cells are separated by the cell spacing value configued in hostconfig",
                                      "wrap": true
                                  }
                              ],
                              "style": "accent"
                          }
                      ]
                  }
              ]
          },
          {
              "type": "TextBlock",
              "text": "Table without gridlines",
              "wrap": true,
              "style": "heading"
          },
          {
              "type": "Table",
              "columns": [
                  {
                      "width": 1
                  },
                  {
                      "width": 1
                  }
              ],
              "rows": [
                  {
                      "type": "TableRow",
                      "cells": [
                          {
                              "type": "TableCell",
                              "items": [
                                  {
                                      "type": "TextBlock",
                                      "text": "showGridLines value",
                                      "wrap": true
                                  }
                              ],
                              "style": "accent"
                          },
                          {
                              "type": "TableCell",
                              "items": [
                                  {
                                      "type": "TextBlock",
                                      "text": "Behavior",
                                      "wrap": true
                                  }
                              ],
                              "style": "accent"
                          }
                      ]
                  },
                  {
                      "type": "TableRow",
                      "cells": [
                          {
                              "type": "TableCell",
                              "items": [
                                  {
                                      "type": "TextBlock",
                                      "text": "true",
                                      "wrap": true
                                  }
                              ],
                              "style": "accent"
                          },
                          {
                              "type": "TableCell",
                              "items": [
                                  {
                                      "type": "TextBlock",
                                      "text": "Cells are separated by 1px width gridlines",
                                      "wrap": true
                                  }
                              ],
                              "style": "accent"
                          }
                      ]
                  },
                  {
                      "type": "TableRow",
                      "cells": [
                          {
                              "type": "TableCell",
                              "items": [
                                  {
                                      "type": "TextBlock",
                                      "text": "false",
                                      "wrap": true
                                  }
                              ],
                              "style": "accent"
                          },
                          {
                              "type": "TableCell",
                              "items": [
                                  {
                                      "type": "TextBlock",
                                      "text": "Cells are separated by the cell spacing value configued in hostconfig",
                                      "wrap": true
                                  }
                              ],
                              "style": "accent"
                          }
                      ]
                  }
              ],
              "showGridLines": false
          }
      ]
  }
   
  
  )

  adaptiveCardShowCardRtl =()=>({
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.5",
    "body": [
        {
            "type": "TextBlock",
            "text": "Please select your language:"
        }
    ],
    "actions": [
        {
            "type": "Action.ShowCard",
            "title": "English",
            "card": {
                "type": "AdaptiveCard",
                "body": [
                    {
                        "type": "TextBlock",
                        "size": "Medium",
                        "weight": "Bolder",
                        "text": " Registration Form",
                        "horizontalAlignment": "Center",
                        "wrap": true,
                        "style": "heading"
                    },
                    {
                        "type": "Input.Text",
                        "label": "Name",
                        "style": "text",
                        "id": "SimpleValENG",
                        "isRequired": true,
                        "errorMessage": "Name is required"
                    },
                    {
                        "type": "Input.Text",
                        "label": "Email",
                        "style": "Email",
                        "id": "EmailValENG"
                    },
                    {
                        "type": "Input.Text",
                        "label": "Phone",
                        "style": "Tel",
                        "id": "TelValENG"
                    },
                    {
                        "type": "Input.Text",
                        "label": "Comments",
                        "style": "text",
                        "isMultiline": true,
                        "id": "MultiLineValENG"
                    }
                ],
                "actions": [
                    {
                        "type": "Action.Submit",
                        "title": "Submit"
                    }
                ]
            }
        },
        {
            "type": "Action.ShowCard",
            "title": "عربي",
            "card": {
                "type": "AdaptiveCard",
                "rtl": true,
                "body": [
                    {
                        "type": "TextBlock",
                        "size": "Medium",
                        "weight": "Bolder",
                        "text": " إستمارة تسجيل",
                        "horizontalAlignment": "Center",
                        "wrap": true,
                        "style": "heading"
                    },
                    {
                        "type": "Input.Text",
                        "label": "اسم",
                        "style": "text",
                        "id": "SimpleValARA",
                        "isRequired": true,
                        "errorMessage": "مطلوب اسم"
                    },
                    {
                        "type": "Input.Text",
                        "label": "بريد الالكتروني",
                        "style": "Email",
                        "id": "EmailValARA"
                    },
                    {
                        "type": "Input.Text",
                        "label": "هاتف",
                        "style": "Tel",
                        "id": "TelValARA"
                    },
                    {
                        "type": "Input.Text",
                        "label": "تعليقات",
                        "style": "text",
                        "isMultiline": true,
                        "id": "MultiLineValARA"
                    }
                ],
                "actions": [
                    {
                        "type": "Action.Submit",
                        "title": "يقدم"
                    }
                ]
            }
        }
    ]
})

  adaptiveCardRtl = () =>({
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.5",
    "rtl": true,
    "body": [
        {
            "type": "TextBlock",
            "size": "medium",
            "weight": "bolder",
            "text": " ${FormInfo.titleARA}",
            "horizontalAlignment": "center",
            "wrap": true,
            "style": "heading"
        },
        {
            "type": "Input.Text",
            "label": "اسم",
            "style": "text",
            "id": "SimpleValARA",
            "isRequired": true,
            "errorMessage": "مطلوب اسم"
        },
        {
            "type": "Input.Text",
            "label": "بريد الالكتروني",
            "style": "email",
            "id": "EmailValARA"
        },
        {
            "type": "Input.Text",
            "label": "هاتف",
            "style": "tel",
            "id": "TelValARA"
        },
        {
            "type": "Input.Text",
            "label": "تعليقات",
            "style": "text",
            "isMultiline": true,
            "id": "MultiLineValARA"
        }
    ],
    "actions": [
        {
            "type": "Action.Submit",
            "title": "يقدم"
        }
    ]
})

  adaptiveCardCarousel = () => ({
  "type": "AdaptiveCard",
  "body": [
    {
      "type": "TextBlock",
      "size": "Medium",
      "weight": "Bolder",
      "text": "{searchword}"
    },
    {
      "type": "ColumnSet",
      "columns": [

        {
          "type": "Column",
          "items": [
            {
              "type": "TextBlock",
              "text": "{advertiser}",
              "size": "Default",
              "wrap": true,
              "advertiserisVisible": "{advvalue}"
            },
            {
              "type": "TextBlock",
              "text": "{creator}",
              "size": "Default",
              "wrap": true,
              "creatorisVisible": "{crtrvvalue}"
            },
            {
              "type": "TextBlock",
              "text": "{market}",
              "size": "Default",
              "wrap": true,
              "marketisVisible": "{mivalue}"
            },
            {
              "type": "TextBlock",
              "text": "{agency}",
              "size": "Default",
              "wrap": true,
              "agencyisVisible": "{agvalue}"
            },
            {
              "type": "TextBlock",
              "text": "{date}",
              "size": "Default",
              "wrap": true,
              "dateisVisible": "{dtvalue}"
            },
            {
              "type": "TextBlock",
              "text": "{mediatype}",
              "size": "Default",
              "wrap": true,
              "mediatypeisVisible": "{mdtypvalue}"
            },
            {
              "type": "TextBlock",
              "text": "{businessunit}",
              "size": "Default",
              "wrap": true,
              "businessunitisVisible": "{buvalue}"
            }
          ]
        }
      ]
    }
  ],
  "actions": [
    {
      "type": "Action.OpenUrl",
      "title": "View in CS Assets",
      "url": "{csasseturl}"
    },
    {
      "type": "Action.Submit",
      "id": "btnPreviewAsset",
      "title": "Preview",
      "data": {
        "data": {
          "id": "assetid",
          "vid": "assetmediaid",
          "filetitle": "assettitleval",
          "mfileid": "masterfileid",
          "actionbutton": "preview"
        },
        "msteams": {
          "type": "task/fetch"
        }
      }
    },
    {
      "type": "Action.Submit",
      "id": "btTag",
      "title": "Select",
      "data": {
        "data": {
          "id": "assetid",
          "vid": "assetmediaid",
          "filetitle": "assettitleval",
          "mfileid": "masterfileid",
          "actionbutton": "tagasset",
          "FileId": "ValFileId",
          "VideoMediaId": "ValVideoMediaId",
          "FileName": "ValFileName",
          "FileAuthor": "ValFileAuthor",
          "FileCreatedDate": "ValFileCreatedDate",
          "FileURL": "ValFileURL",
          "MediaType": "ValMediaType",
          "Advertiser": "ValAdvertiser",
          "Market": "ValMarket",
          "Agency": "ValAgency",
          "BusinessUnit": "ValBusinessUnit",
          "MasterFileId": "ValMasterFileId",
          "commandId": "TAGASSET"
        }
      }
    }
  ],
  "msteams": {    "width": "full"  },
  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
  "version": "1.2"

})
}
  

 


module.exports.TeamsBot = TeamsBot;