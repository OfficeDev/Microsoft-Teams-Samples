const axios = require("axios").default;
const redditOptions = require("./RedditOptions");

class RedditAppAuthenticator {
  static CacheKey = "RedditAppToken";

  constructor() {
    this.redditAppId = redditOptions.redditAppId;
    this.redditAppPassword = redditOptions.redditAppPassword;
    this.cache = new TokenCache();
  }

  async getAccessToken() {
    let accessToken = await this.cache.get(RedditAppAuthenticator.CacheKey);
    if (!accessToken) {
      const response = await axios({
        method: "POST",
        url: "https://www.reddit.com/api/v1/access_token",
        data: new URLSearchParams({
          grant_type: "client_credentials",
          scope: "read",
        }),
        auth: {
          username: this.redditAppId,
          password: this.redditAppPassword,
        },
      });
      accessToken = response.data.access_token;
      // Token is valid for 24hr; cache for half-life (12hr).
      this.cache.setWithTimeout(RedditAppAuthenticator.CacheKey, accessToken, 43200000);
    }
    return accessToken;
  }
}

class TokenCache {
  cache = new Map();

  setWithTimeout(key, value, timeout) {
    if (this.cache.has(key)) {
      clearTimeout(this.cache.get(key).timeout);
    }
    const timeoutId = setTimeout(() => {
      this.cache.delete(key);
    }, timeout);
    this.cache.set(key, { value, timeout: timeoutId });
  }

  get(key) {
    const cachedValue = this.cache.get(key);
    return cachedValue ? cachedValue.value : undefined;
  }
}

module.exports.RedditAppAuthenticator = RedditAppAuthenticator;

