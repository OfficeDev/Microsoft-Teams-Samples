const https = require("https");

/**
 * Reddit App Authenticator
 * Handles authentication with Reddit API
 */
class RedditAppAuthenticator {
  /**
   * Creates an instance of RedditAppAuthenticator
   * @param {RedditOptions} options - Reddit API options
   */
  constructor(options) {
    this.options = options;
    this.accessToken = null;
    this.tokenExpiry = null;
  }

  /**
   * Gets a valid access token, refreshing if necessary
   * @returns {Promise<string>} Access token
   */
  async getAccessToken() {
    // Check if we have a valid token
    if (this.accessToken && this.tokenExpiry && Date.now() < this.tokenExpiry) {
      return this.accessToken;
    }

    // Request a new token
    return new Promise((resolve, reject) => {
      const auth = this.options.getBasicAuth();
      const postData = "grant_type=client_credentials";

      const options = {
        hostname: "www.reddit.com",
        port: 443,
        path: "/api/v1/access_token",
        method: "POST",
        headers: {
          "Authorization": `Basic ${auth}`,
          "Content-Type": "application/x-www-form-urlencoded",
          "Content-Length": Buffer.byteLength(postData),
          "User-Agent": this.options.userAgent,
        },
      };

      const req = https.request(options, (res) => {
        let data = "";

        res.on("data", (chunk) => {
          data += chunk;
        });

        res.on("end", () => {
          if (res.statusCode === 200) {
            try {
              const response = JSON.parse(data);
              this.accessToken = response.access_token;
              // Set expiry to 5 minutes before actual expiry for safety margin
              this.tokenExpiry = Date.now() + (response.expires_in - 300) * 1000;

              resolve(this.accessToken);
            } catch (error) {
              reject(new Error("Failed to parse Reddit API response"));
            }
          } else {
            reject(new Error("Failed to authenticate with Reddit API"));
          }
        });
      });

      req.on("error", (error) => {
        reject(new Error("Failed to authenticate with Reddit API"));
      });

      req.write(postData);
      req.end();
    });
  }
}

module.exports = RedditAppAuthenticator;
