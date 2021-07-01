namespace ReporterPlus.Models
{
    public class ActionType
    {
        public Action action { get; set; }
        public string trigger { get; set; }
    }

    public class Action
    {
        public string type { get; set; }
        public string title { get; set; }
        public RefreshData data { get; set; }
        public string verb { get; set; }
    }

    public class RefreshData
    {
        public string reqId { get; set; }
    }
}
