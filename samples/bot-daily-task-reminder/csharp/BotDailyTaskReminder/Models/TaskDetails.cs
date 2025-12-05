using System.Text.Json.Serialization;

namespace BotDailyTaskReminder.Models
{
    /// <summary>
    /// Task details model class for form submissions.
    /// </summary>
    public class TaskDetails
    {
        /// <summary>
        /// Gets or sets title value of task.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets description value of task.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets date-time value of task.
        /// </summary>
        [JsonPropertyName("dateTime")]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets selected days value of task.
        /// </summary>
        [JsonPropertyName("selectedDays")]
        public string[] SelectedDays { get; set; }
    }
}
