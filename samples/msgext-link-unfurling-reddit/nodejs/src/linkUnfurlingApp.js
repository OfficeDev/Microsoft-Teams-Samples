const { TeamsActivityHandler, CardFactory } = require("botbuilder");
const helloWorldCard = require("./adaptiveCards/helloWorldCard.json");
const {RedditHttpClient} = require("./redditApi/RedditHttpClient");
const ACData = require("adaptivecards-templating");

class LinkUnfurlingApp extends TeamsActivityHandler {
  redditClient;

  constructor(){
    super();
    this.redditClient = new RedditHttpClient();
  }
  // Link Unfurling.
  // This function can be triggered after this app is installed.
  async handleTeamsAppBasedLinkQuery(context, query) {
    const post = await this.redditClient.GetLink(query.url);
    const template = new ACData.Template(helloWorldCard);
    const adaptiveCard = template.expand({
      $root: {
        post: post,
      }
    });

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

  // Zero Install Link Unfurling
  // This function can be triggered if this app sets "supportsAnonymizedPayloads": true in manifest and is uploaded to org's app catalog.
  handleTeamsAnonymousAppBasedLinkQuery(context, query) {
    // When the returned card is an adaptive card, the previewCard property of the attachment is required.
    const previewCard = CardFactory.thumbnailCard("Preview Card", query.url, [
      "https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png",
    ]);

    const attachment = { ...CardFactory.adaptiveCard(helloWorldCard), preview: previewCard };

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
