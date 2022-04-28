using Microsoft.Graph;
using System.Collections.Generic;

namespace MSGraphSearchSample.Models.Search
{
    public class SearchResults
    {
        public List<SearchHit> Hits { get; set; }
        public int Total { get; set; }
        public int From { get; set; }
        public int CurrentPage { get; set; }
        public string Action { get; set; }
        public EntityType EntityType { get; set; }
        public string QueryString { get; set; }
    }
}
