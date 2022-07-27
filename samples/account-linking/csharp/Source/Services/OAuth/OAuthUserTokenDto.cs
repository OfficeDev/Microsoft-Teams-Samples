namespace Microsoft.Teams.Samples.AccountLinking.OAuth;

/// <summary>
/// Storage representation of a User's OAuth2.0 access and refresh tokens.
/// </summary>
internal sealed class OAuthUserTokenDto
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset AccessTokenExpiration { get; set; } = DateTimeOffset.MinValue;

    public string RefreshToken { get; set; } = string.Empty;
}