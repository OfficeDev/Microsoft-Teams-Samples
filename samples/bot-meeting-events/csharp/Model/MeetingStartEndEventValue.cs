using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingEventsCallingBot.Model
{
    public class MeetingStartEndEventValue
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string MeetingType { get; set; }

        public string JoinUrl { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

    }
}
