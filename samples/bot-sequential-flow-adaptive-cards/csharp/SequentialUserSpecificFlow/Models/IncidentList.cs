using System;

namespace SequentialUserSpecificFlow.Models
{
    public class IncidentList
    {
        public IncidentChoiceSet[] incidentList { get; set; }
    }

    public class IncidentChoiceSet
    {
        public string title { get; set; }

        public Guid value { get; set; }
    }

    public class IsBotInstalled
    {
        public bool isBotInstalled { get; set; }
    }
}
