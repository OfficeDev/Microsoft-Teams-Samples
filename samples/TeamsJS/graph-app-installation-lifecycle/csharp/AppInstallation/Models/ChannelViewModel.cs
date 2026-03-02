using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppInstallation.Models
{
    public class AppViewModel
    {
        public List<AppModel> appList { get; set; }
        public List<string> TabsList { get; set; }
        public List<string> Members { get; set; }
        public string TenantId { get; set; }
        public string GroupId { get; set; }
        public bool IsUserList { get; set; }
    }

    public class AppModel
    {     
        public string AppId { get; set; }
        public string AppName { get; set; }
        public string AppDesc { get; set; }
        public string AppDistributionMethod { get; set; }

    }
}
