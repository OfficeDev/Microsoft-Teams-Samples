const axios = require("axios");
const querystring = require("querystring");
const { TeamsActivityHandler, CardFactory } = require("botbuilder");

class TeamsBot extends TeamsActivityHandler {
  constructor() {
    super();
  }

  // Msgext-link-unfurling
  handleTeamsAppBasedLinkQuery() {
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
                "text": "These samples are designed to help understand Microsoft Teams platform capabilities and scenarios(Tabs,Bots,",
                "weight": "bolder",
                "size": "medium"
                }
            ]
         }
        ]
    });

    const attachment = card;

    attachment.preview = {
        content: {
            title: "Thumbnail Card",
            text:"Test",
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
    const searchQuery = query.parameters[0].value;
    const response = await axios.get(
      `http://registry.npmjs.com/-/v1/search?${querystring.stringify({
        text: searchQuery,
        size: 8,
      })}`
    );

    const attachments = [];
    
    response.data.objects.forEach((obj) => {
      const heroCard = CardFactory.heroCard(obj.package.name);
      const preview = CardFactory.heroCard(obj.package.name);
      preview.content.tap = {
        type: "invoke",
        value: { name: obj.package.name, description: obj.package.description },
      };
      const attachment = { ...heroCard, preview };
      attachments.push(attachment);
    });

    return {
      composeExtension: {
        type: "result",
        attachmentLayout: "list",
        attachments: attachments,
      },
    };
  }

  // Receives invoke activities with the name 'selectItem'.
  //Used in creating a Search-based Message Extension. 
  async handleTeamsMessagingExtensionSelectItem(context, obj) {
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
          "text": "Adaptive Card-based Loop components",
          "size": "Large",
          "weight": "Bolder"
          },
          {
          "type": "Container",
          "items": [
              {
              "type": "TextBlock",
              "text": obj.name,
              "weight": "bolder",
              "size": "medium"
              },
              {
                "type": "TextBlock",
                "text": obj.description,
                "weight": "bolder",
                "size": "medium"
                }
          ]
       }
      ]
  });

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
