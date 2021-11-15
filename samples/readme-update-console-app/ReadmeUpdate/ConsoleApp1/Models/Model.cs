using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1.Models
{
    public class RepositoryContent
    {
        public String name;
        public String type;
        public String download_url;
        public LinkFields _links;
        public String sha;
    }

    public class LinkFields
    {
        public String self;
    }

    public class Sample
    {
        public string SampleLinkKey { get; set; }

        public string SampleFolder { get; set; }
    }

    public class UpdateParams
    {
        public string content { get; set; }

        public string message { get; set; }

        public string sha { get; set; }

        public string branch { get; set; }
    }

    public class CommitInformation
    {
        public CommitDetails Commit { get; set; }
    }

    public class CommitDetails
    {
        public Author author { get; set; }
    }

    public class Author
    {
        public string name { get; set; }
        public string email { get; set; }
        public DateTime date { get; set; }
    }
}
