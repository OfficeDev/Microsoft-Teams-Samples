using Microsoft.Graph;
using System.Text.Json.Serialization;

namespace EventMeeting.Models
{
    public class MeetingCreationList
    {
        public string id { get; set; }
        public string topicName { get; set; }
        public string trainerName { get; set; }
        [JsonPropertyName("start")]
        public DateTimeTimeZone Start { get; set; }
        [JsonPropertyName("end")]
        public DateTimeTimeZone End { get; set; }
        [JsonPropertyName("attendees")]
        public IEnumerable<Attendee> Attendees { get; set; }
    }
}
