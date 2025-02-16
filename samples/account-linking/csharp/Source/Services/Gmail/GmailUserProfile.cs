using System.Text.Json.Serialization;

namespace Microsoft.Teams.Samples.AccountLinking.SampleClient.Services.Gmail
{
    public record GmailUserProfile(
        [property: JsonPropertyName("emailAddress")] string emailAddress,
        [property: JsonPropertyName("messagesTotal")] int totalMessages,
        [property: JsonPropertyName("threadsTotal")] int totalThreads,
        [property: JsonPropertyName("historyId")] string historyId
    );
}
