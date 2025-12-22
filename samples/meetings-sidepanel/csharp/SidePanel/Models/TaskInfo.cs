using System.Text.Json.Serialization;

namespace SidePanel.Models
{
    public class TaskInfo
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
    }
}
