namespace SequentialUserSpecificFlow.Models
{
    public class InitialSequentialCard
    {
        public Action action { get; set; }
        public string trigger { get; set; }
    }
    public class Action
    {
        public string type { get; set; }
        public string title { get; set; }
        public Data data { get; set; }
        public string verb { get; set; }
    }
    public class Data
    {
        public string CreatedBy { get; set; }
        public string IncidentTitle { get; set; }
        public string AssignedTo { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string AssignedToName { get; set; }
        public string UserMRI { get; set; }
    }
}
