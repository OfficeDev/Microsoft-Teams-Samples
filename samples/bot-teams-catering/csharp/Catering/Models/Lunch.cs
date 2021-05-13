using Newtonsoft.Json;
using System;

namespace Catering.Models
{
    public class Lunch
    {
        [JsonProperty("orderTimestamp")]
        public DateTime OrderTimestamp { get; set; }

        [JsonProperty("drink")]
        public string Drink { get; set; }

        [JsonProperty("entre")]
        public string Entre { get; set; }
    }
}
