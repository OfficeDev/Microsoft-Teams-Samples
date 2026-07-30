using System;
using System.Text.Json.Serialization;

namespace Microsoft.BotBuilderSamples.SPListBot.Models
{
    /// <summary>
    /// Represents a value/item stored in SharePoint list.
    /// </summary>
    public class Values
    {
        [JsonPropertyName("Username")]
        public string? Username { get; set; }

        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Address")]
        public string? Address { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("Created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("ServerRedirectedEmbedUri")]
        public object? ServerRedirectedEmbedUri { get; set; }

        [JsonPropertyName("ServerRedirectedEmbedUrl")]
        public string? ServerRedirectedEmbedUrl { get; set; }

        [JsonPropertyName("ContentTypeId")]
        public string? ContentTypeId { get; set; }

        [JsonPropertyName("Title")]
        public string? Title { get; set; }

        [JsonPropertyName("AuthorId")]
        public int AuthorId { get; set; }

        [JsonPropertyName("Modified")]
        public DateTime Modified { get; set; }

        [JsonPropertyName("EditorId")]
        public int EditorId { get; set; }

        [JsonPropertyName("OData__UIVersionString")]
        public string? ODataUIVersionString { get; set; }

        [JsonPropertyName("GUID")]
        public string? Guid { get; set; }
    }
}
