using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Content_Bubble_Bot.Models
{
    public class SubmitFeedbackAction : PushAgendaAction
    {
        public string Topic { get; set; }
        public string Feedback { get; set; }
    }
}
