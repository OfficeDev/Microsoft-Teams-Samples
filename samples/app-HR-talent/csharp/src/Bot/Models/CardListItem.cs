using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace TeamsTalentMgmtApp.Bot.Models
{
    public class CardListItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "subtitle")]
        public string Subtitle { get; set; }

        [JsonProperty(PropertyName = "tap")]
        public CardAction Tap { get; set; }
    }
}
