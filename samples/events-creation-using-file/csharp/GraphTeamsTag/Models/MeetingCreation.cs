using Microsoft.Graph;
using System.Text.Json.Serialization;

namespace GraphTeamsTag.Models
{
    public class MeetingCreation
    {
      //  public int Id { get; set; }

        //public string Identity { get; set; }
        public string topicName { get; set; }       
        //public Recipient trainerName { get; set; }
        public string trainerName { get; set; }
        public string startdate { get; set; }
        public string enddate { get; set; }
        public string timing { get; set; }
        public string participants { get; set; }
       
    }

}
