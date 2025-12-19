const axios = require("axios");

/**
 * Reddit HTTP Client
 * Handles HTTP requests to Reddit API
 */
class RedditHttpClient {
  /**
   * Creates an instance of RedditHttpClient
   * @param {RedditAppAuthenticator} authenticator - Reddit authenticator
   * @param {RedditOptions} options - Reddit API options
   */
  constructor(authenticator, options) {
    this.authenticator = authenticator;
    this.options = options;
    this.baseUrl = "https://oauth.reddit.com";
  }

  /**
   * Extracts subreddit and post ID from a Reddit URL
   * @param {string} url - Reddit URL
   * @returns {{subreddit: string, postId: string} | null} Parsed URL components
   */
  parseRedditUrl(url) {
    // Regex patterns for different Reddit URL formats
    const patterns = [
      /reddit\.com\/r\/([^\/]+)\/comments\/([^\/]+)/,
      /redd\.it\/([^\/\?]+)/,
    ];

    for (const pattern of patterns) {
      const match = url.match(pattern);
      if (match) {
        if (pattern.source.includes("redd.it")) {
          return { subreddit: null, postId: match[1] };
        }
        return { subreddit: match[1], postId: match[2] };
      }
    }

    return null;
  }

  /**
   * Gets post information from Reddit
   * @param {string} url - Reddit post URL
   * @returns {Promise<object>} Post data
   */
  async getPostInfo(url) {
    try {
      const parsed = this.parseRedditUrl(url);
      if (!parsed) {
        throw new Error("Invalid Reddit URL");
      }

      const token = await this.authenticator.getAccessToken();

      let apiUrl;
      if (parsed.subreddit) {
        apiUrl = `${this.baseUrl}/r/${parsed.subreddit}/comments/${parsed.postId}`;
      } else {
        // For redd.it links, we need to get the post by ID
        apiUrl = `${this.baseUrl}/api/info?id=t3_${parsed.postId}`;
      }

      const response = await axios.get(apiUrl, {
        headers: {
          Authorization: `Bearer ${token}`,
          "User-Agent": this.options.userAgent,
        },
      });

      // Handle different response formats
      let postData;
      if (parsed.subreddit) {
        // Standard URL format
        postData = response.data[0].data.children[0].data;
      } else {
        // redd.it format
        if (response.data.data.children.length === 0) {
          throw new Error("Post not found");
        }
        postData = response.data.data.children[0].data;
      }

      return this.formatPostData(postData);
    } catch (error) {
      throw error;
    }
  }

  /**
   * Formats Reddit post data for use in adaptive cards
   * @param {object} postData - Raw post data from Reddit API
   * @returns {object} Formatted post data
   */
  formatPostData(postData) {
    return {
      title: postData.title,
      author: postData.author,
      subreddit: postData.subreddit,
      score: postData.score,
      numComments: postData.num_comments,
      created: new Date(postData.created_utc * 1000),
      url: `https://reddit.com${postData.permalink}`,
      selftext: postData.selftext || "",
      thumbnail: postData.thumbnail && postData.thumbnail.startsWith("http") ? postData.thumbnail : null,
      isVideo: postData.is_video || false,
      domain: postData.domain,
    };
  }
}

module.exports = RedditHttpClient;
