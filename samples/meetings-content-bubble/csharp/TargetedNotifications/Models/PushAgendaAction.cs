using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TargetedInMeetingNotificationBot.Models
{
    public class PushAgendaAction : ActionBase
    {
        public string Choice { get; set; }
    }
}
