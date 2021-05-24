using Newtonsoft.Json;

namespace Microsoft.Bot.AdaptiveCards
{
    public class AdaptiveCardInvoke
    {
        [JsonProperty("action")]
        public AdaptiveCardAction Action { get; set; }
    }
}
