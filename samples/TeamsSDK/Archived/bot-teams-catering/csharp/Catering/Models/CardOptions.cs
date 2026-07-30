using Newtonsoft.Json;

namespace Catering.Models
{
    public class CardOptions
    {
        [JsonProperty("nextCardToSend")]
        public int? NextCardToSend { get; set; }

        [JsonProperty("currentCard")]
        public int? CurrentCard { get; set; }

        [JsonProperty("option")]
        public string? Option { get; set; }

        [JsonProperty("custom")]
        public string? Custom { get; set; }
    }
}
