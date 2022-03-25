using Newtonsoft.Json;

namespace SequentialUserSpecificFlow.Models
{
    /// <summary>
    /// Card task fetch value model class.
    /// </summary>
    public class CardTaskFetchValue<T>
    {
        [JsonProperty("incidentId")]
        public string IncidentId { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }
}
