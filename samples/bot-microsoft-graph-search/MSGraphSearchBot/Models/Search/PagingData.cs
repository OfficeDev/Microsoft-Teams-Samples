using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSGraphSearchSample.Models.Search
{
    public class PagingData
    {
        public int From { get; set; }
        public int PageNumber { get; set; }
        public EntityType EntityType { get; set; }
        public string QueryString { get; set; }
    }
}
