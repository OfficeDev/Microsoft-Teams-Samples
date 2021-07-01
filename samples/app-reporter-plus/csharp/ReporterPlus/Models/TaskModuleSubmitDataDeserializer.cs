namespace ReporterPlus.Models
{
    public class TaskModuleSubmitDataDeserializer
    {
        public string commandId { get; set; }
        public ContextData context { get; set; }
        public CompleteData data { get; set; }
    }

    public class ContextData
    {
        public string theme { get; set; }
    }

    public class CompleteData
    {
        public string RequestId { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string[] ImagesOutput { get; set; }
        public string RecorderOutput { get; set; }
        public string Comments { get; set; }
        public string SubmittedByMail { get; set; }
        public Assignedto AssignedTo { get; set; }
    }

    public class Assignedto
    {
        public string displayName { get; set; }
        public string email { get; set; }
        public string objectId { get; set; }
    }
}
