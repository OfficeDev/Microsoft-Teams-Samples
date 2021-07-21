namespace ReporterPlus.Models
{
    public class ActionBase
    {
        public Data data { get; set; }
        public Context context { get; set; }
        public object tabContext { get; set; }
    }

    public class Data
    {
        public string reqId { get; set; }
    }

    public class Context
    {
        public string theme { get; set; }
    }
}
