using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace TeamsTalentMgmtApp.Bot.Models
{
    public class ListCard
    {
        public const string ContentType = "application/vnd.microsoft.teams.card.list";

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "items")]
        public IList<CardListItem> Items { get; set; }

        [JsonProperty(PropertyName = "buttons")]
        public IList<CardAction> Buttons { get; set; }
    }
}
