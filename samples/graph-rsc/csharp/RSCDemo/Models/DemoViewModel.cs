using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSCWithGraphAPI.Models
{
    public class DemoViewModel
    {
        public List<string> Channels { get; set; }

        public List<string> Permissions { get; set; }
    }
}
