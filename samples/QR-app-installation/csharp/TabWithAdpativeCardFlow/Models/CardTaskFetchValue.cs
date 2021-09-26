using Newtonsoft.Json;

namespace QRAppInstallation.Models
{
    public class CardTaskFetchValue<T>
    {
        [JsonProperty("type")]
        public object Type { get; set; } = "task/fetch";

        [JsonProperty("id")]
        public object Id { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
