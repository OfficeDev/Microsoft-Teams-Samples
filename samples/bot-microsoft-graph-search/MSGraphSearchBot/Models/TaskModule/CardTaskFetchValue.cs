using Newtonsoft.Json;

namespace MSGraphSearchSample.Models
{
    public class CardTaskFetchValue<T>
    {
        [JsonProperty("type")]
        public object Type { get; set; } = "task/fetch";

        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("task")]
        public string Task { get; set; }
    }
}
