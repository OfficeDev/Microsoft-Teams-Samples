const axios = require("axios").default;
const redditOptions = require("./RedditOptions");

class RedditAppAuthenticator {
  static CacheKey = 'RedditAppToken';
  redditAppId;
  redditAppPassword;
  cache;

  constructor() {
    this.redditAppId = redditOptions.redditAppId;
    this.redditAppPassword = redditOptions.redditAppPassword;
    this.cache = new TokenCache();
  }

  async getAccessToken() {
    let accessToken = await this.cache.get(RedditAppAuthenticator.CacheKey);
    if (!accessToken) {
      const request = {
        method: 'POST',
        url: 'https://www.reddit.com/api/v1/access_token',
        data: new URLSearchParams({
          grant_type: 'client_credentials',
          scope: 'read',
        }),
        auth: {
          username: this.redditAppId,
          password: this.redditAppPassword,
        },
      };
      const response = await axios(request);
      accessToken = response.data.access_token;
      this.cache.setWithTimeout(RedditAppAuthenticator.CacheKey, accessToken, 43200000); // Duration is 24hr, store in cache for half-life.
    }

    return accessToken;
  }
}
class TokenCache {
  cache = new Map();

  async setWithTimeout(key, value, timeout) {
    if (this.cache.has(key)) {
      clearTimeout(this.cache.get(key).timeout);
    }
    const timeoutId = setTimeout(() => {
      this.cache.delete(key);
    }, timeout);
    this.cache.set(key, { value, timeout: timeoutId });
  }

  async get(key) {
    const cachedValue = this.cache.get(key);
    if (cachedValue) {
      return cachedValue.value;
    }
    return undefined;
  }
}

module.exports.RedditAppAuthenticator = RedditAppAuthenticator;

