const { TeamsActivityHandler, CardFactory, TurnContext } = require("botbuilder");
const ACData = require("adaptivecards-templating");
const helloWorldCard = require("./adaptiveCards/helloWorldCard.json");

class ActionApp extends TeamsActivityHandler {
  constructor() {
    super();

    // Echo bot code
    this.onMessage(async (context, next) => {
      TurnContext.removeRecipientMention(context.activity);
      if (context.activity.text != undefined) {
        const text = context.activity.text.trim().toLocaleLowerCase();
        await context.sendActivity('You said ' + text);
      }
    });
  }

  // Action.
  handleTeamsMessagingExtensionSubmitAction(context, action) {
    // The user has chosen to create a card by choosing the 'Create Card' context menu command.
    const template = new ACData.Template(helloWorldCard);
    const card = template.expand({
      $root: {
        title: action.data.title ?? "",
        subTitle: action.data.subTitle ?? "",
        text: action.data.text ?? "",
      },
    });
    const attachment = CardFactory.adaptiveCard(card);
    return {
      composeExtension: {
        type: "result",
        attachmentLayout: "list",
        attachments: [attachment],
      },
    };
  }
}
module.exports.ActionApp = ActionApp;
