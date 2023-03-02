using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InMeetingNotificationsBot.Models
{
    public class SubmitFeedbackAction : PushAgendaAction
    {
        public string Topic { get; set; }
        public string Feedback { get; set; }
    }
}
