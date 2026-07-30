const fs = require('fs');
const path = require('path');
const { TeamsActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
const AdaptiveCardData = require('adaptivecards-templating');
const DeepLinkTabHelper = require('../pages/DeepLinkTabHelper.js');

// Load the Adaptive Card template once at module load.
const adaptiveCardTemplate = JSON.parse(
    fs.readFileSync(path.join(__dirname, '..', 'resources', 'AdaptiveCard.json'), 'utf8')
);

class DeepLinkTabsnode extends TeamsActivityHandler {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            const conversationType = context.activity.conversation.conversationType;
            let BotsDeepLink, MessagingDeepLink, AdaptiveCardDeepLink;
            let ExtendedDeepLink = '';
            let sidePanelLink = '';

            if (conversationType === 'channel') {
                const channelId = context.activity.channelData.teamsChannelId;
                BotsDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel('topic1', 1, 'Bots', channelId, process.env.TeamsAppId, process.env.Channel_Entity_Id);
                MessagingDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel('topic2', 2, 'Messaging Extension', channelId, process.env.TeamsAppId, process.env.Channel_Entity_Id);
                AdaptiveCardDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel('topic3', 3, 'Adaptive Card', channelId, process.env.TeamsAppId, process.env.Channel_Entity_Id);
                ExtendedDeepLink = DeepLinkTabHelper.GetDeepLinkTabChannel('', 4, 'Extended Deeplink features', channelId, process.env.TeamsAppId, process.env.Channel_Entity_Id);
            } else {
                BotsDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic('topic1', 1, 'Bots', process.env.TeamsAppId);
                MessagingDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic('topic2', 2, 'Messaging Extension', process.env.TeamsAppId);
                AdaptiveCardDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic('topic3', 3, 'Adaptive Card', process.env.TeamsAppId);
                ExtendedDeepLink = DeepLinkTabHelper.GetDeepLinkTabStatic('', 4, 'Extended Deeplink features', process.env.TeamsAppId);
                sidePanelLink = DeepLinkTabHelper.GetDeepLinkToMeetingSidePanel(5, 'Side pannel Deeplink', process.env.TeamsAppId, process.env.Base_URL, context.activity.conversation.id, 'chat');
            }

            await context.sendActivity({
                attachments: [this.createAdaptiveCard(BotsDeepLink, MessagingDeepLink, AdaptiveCardDeepLink, ExtendedDeepLink, sidePanelLink)]
            });

            await next();
        });

        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            const welcomeText = 'Hello and welcome!';
            for (let cnt = 0; cnt < membersAdded.length; ++cnt) {
                if (membersAdded[cnt].id !== context.activity.recipient.id) {
                    await context.sendActivity(MessageFactory.text(welcomeText, welcomeText));
                }
            }
            await next();
        });
    }

    createAdaptiveCard(BotsDeepLink, MessagingDeepLink, AdaptiveCardDeepLink, ExtendedDeepLink, sidePanelLink) {
        const template = new AdaptiveCardData.Template(adaptiveCardTemplate);
        const cardPayload = template.expand({
            $root: {
                BotsDeepLink: BotsDeepLink.linkUrl,
                MEDeepLink: MessagingDeepLink.linkUrl,
                CardsDeepLink: AdaptiveCardDeepLink.linkUrl,
                BotsTitle: BotsDeepLink.TaskText,
                METitle: MessagingDeepLink.TaskText,
                CardsTitle: AdaptiveCardDeepLink.TaskText,
                ExtendedDeepLink: ExtendedDeepLink.linkUrl,
                ExtendedDeepLinkTitle: ExtendedDeepLink.TaskText,
                sidePanelLink: sidePanelLink ? sidePanelLink.linkUrl : '',
                sidePanelLinkTitle: sidePanelLink ? sidePanelLink.TaskText : ''
            }
        });
        return CardFactory.adaptiveCard(cardPayload);
    }
}

module.exports = DeepLinkTabsnode;