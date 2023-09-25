using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSCWithGraphAPI.Models
{
    public class ResponseData
    {
        public List<AppData> Value { get; set; }
    }

    public class AppData
    {
        public string Id { get; set; }

        public AppDefination TeamsAppDefinition { get; set; }

    }

    public class AppDefination
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }
    }
}
