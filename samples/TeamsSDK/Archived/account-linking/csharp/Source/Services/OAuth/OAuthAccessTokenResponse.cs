using System.Text.Json.Serialization;

namespace Microsoft.Teams.Samples.AccountLinking.OAuth;

/// <summary>
/// The OAuth2.0 Access Token Response
/// <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.4" />
/// </summary>
public sealed class OAuthAccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public long ExpiresInSeconds { get; set; } = 0;
}