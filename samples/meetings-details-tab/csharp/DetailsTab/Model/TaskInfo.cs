using Microsoft.Extensions.Configuration;
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

    public class TaskInfoList
    {
        public List<TaskInfo> taskInfoList { get; set; } = new List<TaskInfo>();
        public string baseUrl { get; set; }


    }
}
