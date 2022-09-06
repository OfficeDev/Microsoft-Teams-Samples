using Microsoft.Graph;
using System.Text.Json.Serialization;

namespace EventMeeting.Models
{
    public class Meeting
    {     
        //topic name for the subject of meeting
        public string topicName { get; set; }
        //trainer name for the who will give the training of the particular training
        public string trainerName { get; set; }
        //start date of training
        public string startdate { get; set; }
        //end date of the training
        public string enddate { get; set; }
        //timing of the meting
        public string timing { get; set; }
        //participants of the meeting.
        public string participants { get; set; }
       
    }

}
