using Newtonsoft.Json;

namespace QRAppInstallation.Models
{
    public class AppInstallDetails<T>
    {

        [JsonProperty("appid")]
        public object Id { get; set; }

        [JsonProperty("teamid")]
        public object TeamId { get; set; }
    }
}
