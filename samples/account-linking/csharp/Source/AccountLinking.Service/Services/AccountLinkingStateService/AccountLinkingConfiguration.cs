using System.Text.Json;

namespace Microsoft.Teams.Samples.AccountLinking.Service.State;

public class AccountLinkingConfiguration
{
    public IDictionary<string, JsonElement> ConnectionConfigurations {get;set;} = new Dictionary<string, JsonElement>();
}