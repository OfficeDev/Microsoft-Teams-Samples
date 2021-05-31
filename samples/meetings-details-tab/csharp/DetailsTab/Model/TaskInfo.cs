using System.Collections.Generic;

namespace DetailsTab.Models
{
    public class TaskInfo
    {
        public string id { get;  set; }
        public string title { get; set; }
        public string option1 { get; set; }
        public string option2 { get; set; }
        public bool IsSent { get; set; }
        public Dictionary<string, List<string>> PersonAnswered { get; set; }
    }
    public class ActionBase
    {
        public Data data { get; set; }
        public Context context { get; set; }
        public object tabContext { get; set; }
    }

    public class Data
    {
        public string Type { get; set; }
        public string type { get; set; }
    }

    public class Context
    {
        public string theme { get; set; }
    }
}
