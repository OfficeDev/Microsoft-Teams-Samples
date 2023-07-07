const { TeamsActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
var AdaptiveCardData = require("adaptivecards-templating");
var DeepLinkTabHelper = require("../pages/DeepLinkTabHelper.js");
this.AppID = process.env.MicrosoftAppId;

class DeepLinkTabsnode extends TeamsActivityHandler {
  constructor() {
    super();
    this.onMessage(async (context, next) => {
      const replyText = `Echo: ${context.activity.text}`;
      let conversationType = context.activity.conversation.conversationType;
      let ExtendedDeepLink = "";
      let sidePanelLink = "";

      if (context.activity.conversation.conversationType === "channel") {
        var BotsDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel("topic1", 1, "Bots", context.activity.channelData.teamsChannelId, process.env.MicrosoftAppId, process.env.Channel_Entity_Id);
        var MessagingDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel("topic2", 2, "Messaging Extension", context.activity.channelData.teamsChannelId, process.env.MicrosoftAppId, process.env.Channel_Entity_Id);
        var AdaptiveCardDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel("topic3", 3, "Adaptive Card", context.activity.channelData.teamsChannelId, process.env.MicrosoftAppId, process.env.Channel_Entity_Id);
        ExtendedDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel("", 4, "Extended Deeplink features", context.activity.channelData.teamsChannelId, process.env.MicrosoftAppId, process.env.Channel_Entity_Id);
      }
      else {
        var BotsDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic("topic1", 1, "Bots", process.env.MicrosoftAppId);
        var MessagingDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic("topic2", 2, "Messaging Extension", process.env.MicrosoftAppId);
        var AdaptiveCardDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic("topic3", 3, "Adaptive Card", process.env.MicrosoftAppId);
        ExtendedDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic("", 4, "Extended Deeplink features", process.env.MicrosoftAppId);
        sidePanelLink = DeepLinkTabHelper.GetDeepLinkToMeetingSidePanel(5,"Side pannel Deeplink",process.env.MicrosoftAppId, process.env.Base_URL, context.activity.conversation.id, "chat");
      }

      await context.sendActivity({ attachments: [this.createAdaptiveCard(conversationType, BotsDeepLink, MessagingDeepLink, AdaptiveCardDeepLink, ExtendedDeepLink, sidePanelLink)] });
    });

    this.onMembersAdded(async (context, next) => {
      const membersAdded = context.activity.membersAdded;
      const welcomeText = 'Hello and welcome!';
      for (let cnt = 0; cnt < membersAdded.length; ++cnt) {
        if (membersAdded[cnt].id !== context.activity.recipient.id) {
          await context.sendActivity(MessageFactory.text(welcomeText, welcomeText));
        }
      }
      // By calling next() you ensure that the next BotHandler is run.
      await next();
    });
  };

  createAdaptiveCard(conversationType, BotsDeepLink, MessagingDeepLink, AdaptiveCardDeepLink, ExtendedDeepLink, sidePanelLink) {
    const fs = require('fs')
    const jsonContentStr = fs.readFileSync('resources/AdaptiveCard.json', 'utf8')
    var templatePayload = JSON.parse(jsonContentStr);
    var template = new AdaptiveCardData.Template(templatePayload);

    var cardPayload = template.expand({
      $root: {
        BotsDeepLink: BotsDeepLink.linkUrl,
        MEDeepLink: MessagingDeepLink.linkUrl,
        CardsDeepLink: AdaptiveCardDeepLink.linkUrl,
        BotsTitle: BotsDeepLink.TaskText,
        METitle: MessagingDeepLink.TaskText,
        CardsTitle: AdaptiveCardDeepLink.TaskText,
        ExtendedDeepLink: ExtendedDeepLink.linkUrl,
        ExtendedDeepLinkTitle: ExtendedDeepLink.TaskText,
        sidePanelLink: sidePanelLink.linkUrl,
        sidePanelLinkTitle: sidePanelLink.TaskText
      }
    });
    return CardFactory.adaptiveCard(cardPayload);
  }
}

module.exports = DeepLinkTabsnode;