using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeamsToDoAppConnector.Models
{
    public class WebhookDetails
    {
        [JsonProperty("webhookUrl")]
        public string? WebhookUrl { get; set; }

        [JsonProperty("eventType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EventType EventType { get; set; }
    }
}