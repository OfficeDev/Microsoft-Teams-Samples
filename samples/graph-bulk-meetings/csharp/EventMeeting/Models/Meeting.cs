using Microsoft.Graph;
using System.Text.Json.Serialization;

namespace EventMeeting.Models
{
    public class Meeting
    {     
        //topic name for the subject of meeting
        public string TopicName { get; set; }

        //trainer name for the who will give the training of the particular training
        public string TrainerName { get; set; }

        //start date of training
        public string StartDate { get; set; }

        //end date of the training
        public string EndDate { get; set; }

        //timing of the meting
        public string Timing { get; set; }

        //participants of the meeting.
        public string Participants { get; set; }
       
    }

}
