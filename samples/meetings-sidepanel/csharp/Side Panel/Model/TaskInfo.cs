using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SidePanel.Models
{
    public class TaskInfo
    {
        public string title { get; set; }
    }

    public class taskList
    {
        public List<TaskInfo> taskDataList { get; set; }
    }
}