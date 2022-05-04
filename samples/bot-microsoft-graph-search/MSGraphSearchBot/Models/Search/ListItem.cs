using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSGraphSearchSample.Models.Search
{
    public class ListItem
    {
        public string Title { get; set; }
        public string Created { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedTime { get; set; }
        public string URL { get; set; }
        public string SitePath { get; set; }
        public string SiteName { get; set; }
    }
}
