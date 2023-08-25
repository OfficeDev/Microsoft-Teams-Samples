const { TeamsActivityHandler, CardFactory } = require("botbuilder");

const card = CardFactory.adaptiveCard({
  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
  "type": "AdaptiveCard",
  "version": "1.6",
  "metadata": {
      "webUrl": "https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main"
  },
  "body": [
      {
      "type": "TextBlock",
      "text": "Adaptive Card-based Loop component",
      "size": "Large",
      "weight": "Bolder"
      },
      {
      "type": "Container",
      "items": [
          {
          "type": "TextBlock",
          "text": "These samples are designed to help understand Microsoft Teams platform capabilities and scenarios(Bots,Tabs,Message extensions,Meeting extensions,Personal apps,Webhooks and connectors)",
          "weight": "bolder",
          "size": "medium"
          }
      ]
    }
  ],
  "actions": [
    {
      "type": "Action.Execute",
      "title": "Execute!",
      "verb": "userExecute",
      "fallback": "Action.Submit"
    },
    {
      "type": "Action.OpenUrl",
      "title": "Universal Actions for Adaptive Cards",
      "url": "https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/universal-actions-for-adaptive-cards/work-with-universal-actions-for-adaptive-cards"
    },
    {
      "type": "Action.OpenUrl",
      "title": "Adaptive Card-based Loop components",
      "url": "https://learn.microsoft.com/en-us/microsoftteams/platform/m365-apps/cards-loop-component?branch=pr-en-us-9230"
    }
  ]
});

class TeamsBot extends TeamsActivityHandler {
  constructor() {
    super();
   
  }

  // Invoked when an action is taken on an Adaptive Card. The Adaptive Card sends an event to the Bot and this
  // method handles that event.
  async onAdaptiveCardInvoke(context, invokeValue) {
    // The verb "userExecute" is sent from the Adaptive Card defined in adaptiveCards/learn.json
      if (invokeValue.action.verb === "userExecute") {
        const card = {
          "type": "AdaptiveCard",
          "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
          "version": "1.5",
          "body": [
              {
                  "type": "TextBlock",
                  "size": "Default",
                  "text": "Adaptive Card-based Loop component Successfully Execute!! ",
                  "style": "heading"
              },
              {
                  "type": "Image",
                  "url": "https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png",
                  "height": "auto",
                  "size": "Medium",
                  "horizontalAlignment": "left",
                  "spacing": "None",
                  "width": "0px"
              }
          ]
      };
      return {
          statusCode: 200,
          type: "application/vnd.microsoft.card.adaptive",
          value: card
      };
    }
  }

  // Msgext-link-unfurling
  handleTeamsAppBasedLinkQuery() {

    const attachment = JSON.parse(JSON.stringify(card));

    attachment.preview = {
        content: {
            title: "Adaptive Card-based Loop component",
            text:"These samples are designed to help understand Microsoft Teams platform capabilities and scenarios(Bots,Tabs,Message extensions,Meeting extensions,Personal apps,Webhooks and connectors)",
        },
        contentType: "application/vnd.microsoft.card.thumbnail",
    }

    const result = {
        attachmentLayout: 'list',
        type: 'result',
        attachments: [attachment]
    };

    const response = {
        composeExtension: result
    };

    return response;
}

  // Message extension Code Search
  // Used in creating a Search-based Message Extension
  async handleTeamsMessagingExtensionQuery(context, query) {
   
   const attachment = JSON.parse(JSON.stringify(card));

    attachment.preview = {
        content: {
          title: "Adaptive Card-based Loop component",
          text:"These samples are designed to help understand Microsoft Teams platform capabilities and scenarios(Bots,Tabs,Message extensions,Meeting extensions,Personal apps,Webhooks and connectors)",
        },
        contentType: "application/vnd.microsoft.card.thumbnail",
    }
    
    return {
      composeExtension: {
        type: "result",
        attachmentLayout: "list",
        attachments: [attachment],
      },
    };
  }

  // Receives invoke activities with the name 'selectItem'.
  //Used in creating a Search-based Message Extension. 
  async handleTeamsMessagingExtensionSelectItem(context, obj) {
  
      return {
      composeExtension: {
        type: "result",
        attachmentLayout: "list",
        attachments: [JSON.parse(JSON.stringify(card))],
      },
  };
  }
}

module.exports.TeamsBot = TeamsBot;
