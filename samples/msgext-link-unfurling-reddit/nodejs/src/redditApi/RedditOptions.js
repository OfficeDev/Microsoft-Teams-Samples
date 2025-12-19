/**
 * Reddit API Options
 * Contains configuration for Reddit API requests
 */
class RedditOptions {
  /**
   * Creates an instance of RedditOptions
   * @param {string} clientId - Reddit application client ID
   * @param {string} clientSecret - Reddit application client secret
   */
  constructor(clientId, clientSecret) {
    this.clientId = clientId;
    this.clientSecret = clientSecret;
    this.userAgent = "MSTeams:com.microsoft.teams.sample.linkUnfurling:v1.0.0";
  }

  /**
   * Get the base64 encoded credentials for Basic Auth
   * @returns {string} Base64 encoded credentials
   */
  getBasicAuth() {
    return Buffer.from(`${this.clientId}:${this.clientSecret}`).toString("base64");
  }
}

module.exports = RedditOptions;
