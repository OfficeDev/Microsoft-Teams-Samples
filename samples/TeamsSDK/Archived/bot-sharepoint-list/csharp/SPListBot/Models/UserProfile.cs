using System.Text.Json.Serialization;

namespace Microsoft.BotBuilderSamples.SPListBot.Models
{
    /// <summary>
    /// Represents user profile information collected from adaptive card.
    /// </summary>
    public class UserProfile
    {
        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Address")]
        public string? Address { get; set; }

        [JsonPropertyName("PromptedUserForName")]
        public bool PromptedUserForName { get; set; }
    }
}
