using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace TeamsConversationBot.Bots.Models.Streaming
{
    public class ChannelData
    {
        [JsonProperty(PropertyName = "streamId")]
        public string StreamId { get; set; }

        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        [JsonProperty(PropertyName = "streamType")]
        public StreamType StreamType { get; set; }

        [JsonProperty(PropertyName = "streamSequence")]
        public int StreamSequence { get; set; }
    }
}
