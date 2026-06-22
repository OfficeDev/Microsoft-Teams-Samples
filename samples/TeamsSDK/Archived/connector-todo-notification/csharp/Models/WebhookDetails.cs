using System.Text.Json.Serialization;

namespace TeamsToDoAppConnector.Models
{
    public class WebhookDetails
    {
        [JsonPropertyName("webhookUrl")]
        public string? WebhookUrl { get; set; }

        [JsonPropertyName("eventType")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EventType EventType { get; set; }
    }
}