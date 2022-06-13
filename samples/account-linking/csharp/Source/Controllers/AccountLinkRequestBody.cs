using System.Text.Json.Serialization;

namespace Microsoft.Teams.Samples.AccountLinking.Controllers;

public class AccountLinkRequestBody
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("code_verifier")]
    public string? CodeVerifier { get; set; }
}
