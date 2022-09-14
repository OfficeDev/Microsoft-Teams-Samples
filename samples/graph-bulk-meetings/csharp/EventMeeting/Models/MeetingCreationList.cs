using Microsoft.Graph;
using System.Text.Json.Serialization;

namespace EventMeeting.Models
{
    public class MeetingCreationList
    {
        public string Id { get; set; }

        public string TopicName { get; set; }


        [JsonPropertyName("createdDateTime")]
        public DateTimeOffset? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets organizer.
        /// </summary>
        [JsonPropertyName("organizer")]
        public Recipient Organizer { get; set; }

         public EmailAddress EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets web link.
        /// </summary>      
        public string MeetingLink { get; set; }

        [JsonPropertyName("start")]
        public DateTimeTimeZone Start { get; set; }

        [JsonPropertyName("end")]
        public DateTimeTimeZone End { get; set; }

        [JsonPropertyName("attendees")]
        public IEnumerable<Attendee> Attendees { get; set; }
    }
}
