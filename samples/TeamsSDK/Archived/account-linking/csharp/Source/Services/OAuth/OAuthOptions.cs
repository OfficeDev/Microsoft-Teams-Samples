using System.ComponentModel.DataAnnotations;

namespace Microsoft.Teams.Samples.AccountLinking.OAuth;

/// <summary>
/// Generic configuration for the downstream OAuth2.0 service.
/// </summary>
public sealed class OAuthOptions
{
    /// <summary>
    /// The authorization url to redirect user-agents to to start the authorization process for this client.
    /// <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-4.1" />
    /// </summary>
    /// <value>A string representation of the url. </value>
    [Required(AllowEmptyStrings = false)]
    public string AuthorizeUrl { get; set; } = string.Empty;

    /// <summary>
    /// The access token url where this client can exchange codes for access tokens.
    /// <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-4.1" />
    /// </summary>
    /// <value>A string representation of the url.</value>
    [Required(AllowEmptyStrings = false)] 
    public string AccessTokenUrl { get; set; } = string.Empty;

    /// <summary>
    /// The client id registered with the OAuth2.0 authorization server
    /// <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-2.2" />
    /// </summary>
    /// <value>The OAuth2.0 client id.</value>
    [Required(AllowEmptyStrings = false)]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The client secret / password used for authenticating OAuth2.0 operations from this client.
    /// <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-2.3" />
    /// </summary>
    /// <value>The OAuth2.0 client id.</value>
    [Required(AllowEmptyStrings = false)]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// The uri of the start of the auth flow on this client.
    /// </summary>
    /// <remarks>
    /// This needs to be a valid url for the <see cref="OAuthController" />'s StartAuth action.
    /// </remarks>
    [Required(AllowEmptyStrings = false)]
    public string AuthStartUri { get; set; } = string.Empty;

    /// <summary>
    /// The uri of the end of the auth flow on this client.
    /// </summary>
    /// <remarks>
    /// This needs to be a valid url for the <see cref="OAuthController" />'s EndAuthAsync action.
    /// </remarks>
    [Required(AllowEmptyStrings = false)]
    public string AuthEndUri { get; set; } = string.Empty; 

    [Required(AllowEmptyStrings = false)]
    public string AuthEndRedirect {get; set; } = string.Empty;
}