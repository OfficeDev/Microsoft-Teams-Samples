const { TeamsActivityHandler, CardFactory } = require("botbuilder");
const helloWorldCard = require("./adaptiveCards/helloWorldCard.json");
const { RedditHttpClient } = require("./redditApi/RedditHttpClient");
const ACData = require("adaptivecards-templating");

class LinkUnfurlingApp extends TeamsActivityHandler {
  constructor() {
    super();
    this.redditClient = new RedditHttpClient();
  }

  // Link Unfurling — triggered after this app is installed.
  async handleTeamsAppBasedLinkQuery(context, query) {
    return this._buildRedditCardResponse(query.url);
  }

  // Zero Install Link Unfurling — triggered when "supportsAnonymizedPayloads": true in manifest
  // and the app is uploaded to the org's app catalog.
  async handleTeamsAnonymousAppBasedLinkQuery(context, query) {
    return this._buildRedditCardResponse(query.url);
  }

  async _buildRedditCardResponse(url) {
    const post = await this.redditClient.GetLink(url);
    const template = new ACData.Template(helloWorldCard);
    const adaptiveCard = template.expand({ $root: { post } });
    const previewCard = CardFactory.heroCard(post.title, post.subreddit, [post.thumbnail]);
    const attachment = { ...CardFactory.adaptiveCard(adaptiveCard), preview: previewCard };

    return {
      composeExtension: {
        type: "result",
        attachmentLayout: "list",
        attachments: [attachment],
        suggestedActions: {
          actions: [
            {
              title: "default",
              type: "setCachePolicy",
              value: '{"type":"no-cache"}',
            },
          ],
        },
      },
    };
  }
}

module.exports.LinkUnfurlingApp = LinkUnfurlingApp;
