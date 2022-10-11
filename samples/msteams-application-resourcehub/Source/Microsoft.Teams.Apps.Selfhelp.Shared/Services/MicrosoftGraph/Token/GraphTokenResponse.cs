namespace Microsoft.Teams.Apps.Selfhelp.Shared.Services.MicrosoftGraph.Token
{
    using Newtonsoft.Json;

    /// <summary>
    /// Model for the microsoft graph token response.
    /// </summary>
    public class GraphTokenResponse
    {
        /// <summary>
        /// Gets or sets the token type.
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets the duration as to when the token will expire.
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}