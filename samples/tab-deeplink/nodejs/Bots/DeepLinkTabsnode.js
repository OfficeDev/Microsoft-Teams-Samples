// Aggregated module object pattern as requested
const AgentsHosting = require('@microsoft/agents-hosting');
const { ActivityHandler, MessageFactory, CardFactory } = AgentsHosting;
const AdaptiveCardData = require('adaptivecards-templating');
const DeepLinkTabHelper = require('../pages/DeepLinkTabHelper.js');
this.AppID = process.env.MicrosoftAppId;

class DeepLinkTabsnode extends ActivityHandler {
  constructor() {
    super();
    this.onMessage(async (context, next) => {
      console.log('[handler:onMessage] Triggered');
      const replyText = `Echo: ${context.activity.text}`;
      let conversationType = context.activity.conversation.conversationType;
      let ExtendedDeepLink = "";
      let sidePanelLink = "";

      const TEAMS_APP_ID = process.env.TeamsAppId || process.env.TEAMS_APP_ID;
      if (!TEAMS_APP_ID) {
        console.warn('[handler:onMessage] TEAMS_APP_ID missing - deep link generation will fail. Set TEAMS_APP_ID in env.');
      }
      const CHANNEL_ENTITY_ID = process.env.Channel_Entity_Id || process.env.CHANNEL_ENTITY_ID;
      if (!CHANNEL_ENTITY_ID) {
        console.warn('[handler:onMessage] Channel_Entity_Id missing - using placeholder for deep link features.');
      }

      if (context.activity.conversation.conversationType === "channel") {
        var BotsDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel("topic1", 1, "Bots", context.activity.channelData.teamsChannelId, TEAMS_APP_ID, CHANNEL_ENTITY_ID || 'channelEntity');
        var MessagingDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel("topic2", 2, "Messaging Extension", context.activity.channelData.teamsChannelId, TEAMS_APP_ID, CHANNEL_ENTITY_ID || 'channelEntity');
        var AdaptiveCardDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel("topic3", 3, "Adaptive Card", context.activity.channelData.teamsChannelId, TEAMS_APP_ID, CHANNEL_ENTITY_ID || 'channelEntity');
        ExtendedDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel("", 4, "Extended Deeplink features", context.activity.channelData.teamsChannelId, TEAMS_APP_ID, CHANNEL_ENTITY_ID || 'channelEntity');
      }
      else {
        var BotsDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic("topic1", 1, "Bots", TEAMS_APP_ID);
        var MessagingDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic("topic2", 2, "Messaging Extension", TEAMS_APP_ID);
        var AdaptiveCardDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic("topic3", 3, "Adaptive Card", TEAMS_APP_ID);
        ExtendedDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic("", 4, "Extended Deeplink features", TEAMS_APP_ID);
        sidePanelLink = DeepLinkTabHelper.GetDeepLinkToMeetingSidePanel(5,"Side pannel Deeplink",TEAMS_APP_ID, process.env.Base_URL, context.activity.conversation.id, "chat");
      }

      try {
        const cardAttachment = this.createAdaptiveCard(conversationType, BotsDeepLink, MessagingDeepLink, AdaptiveCardDeepLink, ExtendedDeepLink, sidePanelLink);
        // Agents SDK requires an outbound activity to specify type explicitly when sending object
        await context.sendActivity({ type: 'message', attachments: [cardAttachment] });
        console.log('[handler:onMessage] Adaptive card sent');
      } catch (err) {
        console.error('[handler:onMessage] Failed to send adaptive card:', err);
        // Fallback: send plain text so user gets a response
        try {
          await context.sendActivity({ type: 'message', text: replyText });
          console.log('[handler:onMessage] Fallback text sent');
        } catch (inner) {
          console.error('[handler:onMessage] Fallback text failed:', inner);
        }
      }
    });

    this.onMembersAdded(async (context, next) => {
      console.log('[handler:onMembersAdded] Triggered');
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