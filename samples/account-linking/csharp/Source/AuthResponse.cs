using System.Text.Json.Serialization;

namespace Microsoft.Teams.Samples.AccountLinking;

/// <summary>
/// The wrapper format for the auth response we get from the 'authEnd' page's call to notify success
/// </summary>
public sealed class AuthResponse
{
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// The account linking state which gets encoded as the 'code' property in the json payload
    /// This is to maintain a parallel structure to the OAuth2.0 code flow with state & code being returned
    /// </summary>
    /// <value></value>
    [JsonPropertyName("code")]
    public string AccountLinkingState { get; set; } = string.Empty;
}