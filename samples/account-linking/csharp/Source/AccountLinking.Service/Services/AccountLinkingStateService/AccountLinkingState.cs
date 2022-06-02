using System.Text.Json;

namespace Microsoft.Teams.Samples.AccountLinking.Service.State;

/// <summary>
/// The data that needs to be carried through the OAuth flow for later use when the login/consent is completed.
/// 
/// This class wraps around an opaque 'State' which can be used for the mutable values we need to get / set between
/// stages in the auth flow(s)
/// </summary>
public class AccountLinkingState
{
    public string Id { get; set; } = string.Empty;

    public Queue<string> RemainingConnections { get; set; } = new Queue<string>();

    public IDictionary<string, JsonElement> ConnectionConfigurations { get; set; } = new Dictionary<string, JsonElement>();

    public IDictionary<string, string> ConnectionIds { get; set; } = new Dictionary<string, string>();

    public IDictionary<string, byte[]> ConnectionStates { get; set; } = new Dictionary<string, byte[]>();

    public string ClientState { get; set; } = string.Empty;

    public string CodeChallenge { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;
}