using Newtonsoft.Json;

namespace QRAppInstallation.Models
{
    public class AppInstallDetails<T>
    {

        [JsonProperty("appid")]
        public object AppId { get; set; }

        [JsonProperty("teamid")]
        public object TeamId { get; set; }
    }
}
