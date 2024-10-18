using Newtonsoft.Json;

namespace MeetingAutoRecording.Models
{
    public class ResourceDatas
    {
        // The ID of the resource.
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        // The OData etag property.
        [JsonProperty(PropertyName = "@odata.type")]
        public string ODataEType { get; set; }

        // The OData ID of the resource. This is the same value as the resource property.
        [JsonProperty(PropertyName = "@odata.id")]
        public string ODataId { get; set; }
    }
}
