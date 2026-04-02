const axios = require("axios").default;
const { RedditAppAuthenticator } = require("./RedditAppAuthenticator");

class RedditHttpClient {
  parameterExtractor = /https?:\/\/\w*\.?reddit\.com\/r\/(?<subreddit>\w+)\/comments\/(?<id>\w+)/;

  constructor() {
    this.redditAuthenticator = new RedditAppAuthenticator();
  }

  async GetLink(url) {
    const match = this.parameterExtractor.exec(url);
    if (!match) return undefined;

    const { subreddit, id } = match.groups;
    return this.GetLinkModel(subreddit, id);
  }

  async GetLinkModel(subreddit, id) {
    const authToken = await this.redditAuthenticator.getAccessToken();
    const response = await axios.get(
      `https://oauth.reddit.com/r/${encodeURIComponent(subreddit)}/api/info?id=t3_${encodeURIComponent(id)}`,
      { headers: { Authorization: `Bearer ${authToken}` } }
    );
    const firstPost = response.data.data.children[0].data;

    return {
      title: firstPost.title,
      id: firstPost.id,
      score: firstPost.score,
      subreddit: firstPost.subreddit,
      thumbnail: firstPost.preview?.images[0]?.source.url.replace(/&amp;/g, "&"),
      selfText: firstPost.selftext,
      numComments: firstPost.num_comments,
      link: "https://www.reddit.com" + firstPost.permalink,
      author: firstPost.author,
      created: new Date(firstPost.created_utc * 1000).toISOString(),
    };
  }
}

module.exports.RedditHttpClient = RedditHttpClient;