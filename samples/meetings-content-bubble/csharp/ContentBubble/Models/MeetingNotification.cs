using System.Collections.Generic;

namespace Content_Bubble_Bot.Models
{
    public class MeetingNotification
    {   
        /// <summary>
        /// List of participants that are currently part of the meeting.
        /// </summary>
        public List<ParticipantDetail> ParticipantDetails { get; set; }
    }
}