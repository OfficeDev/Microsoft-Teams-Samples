using System;

namespace SequentialUserSpecificFlow.Models
{
    public class IncidentDetails
    {
        public Guid IncidentId { get; set; }
        public string CreatedBy { get; set; }
        public string IncidentTitle { get; set; }
        public string AssignedToMRI { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string AssignedToName { get; set; }
    }
}
