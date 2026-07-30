namespace Microsoft.Teams.Samples.AccountLinking.State;

/// <summary>
/// The data that needs to be carried through the OAuth flow for later use when the login/consent is completed.
/// 
/// This class wraps around an opaque 'State' which can be used for the mutable values we need to get / set between
/// stages in the auth flow(s)
/// </summary>
public sealed class AccountLinkingState
{
    public string Id { get; } = string.Empty;

    public string CodeChallenge { get; } = string.Empty;

    public string OAuthCode { get; set; } = string.Empty;

    public string ClientState { get; set; } = string.Empty;

    public AccountLinkingState(string codeChallenge, string? id = default)
    {
        Id = id ?? Guid.NewGuid().ToString();
        CodeChallenge = codeChallenge;
    }
}