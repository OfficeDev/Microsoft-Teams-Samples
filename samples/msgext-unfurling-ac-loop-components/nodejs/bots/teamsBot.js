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
  ]
});

class TeamsBot extends TeamsActivityHandler {
  constructor() {
    super();
  }

  // Msgext-link-unfurling
  handleTeamsAppBasedLinkQuery() {

    const attachment = card;

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
   
   const attachment = card;

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
      attachments: [card],
    },
  };
  }
}

module.exports.TeamsBot = TeamsBot;
