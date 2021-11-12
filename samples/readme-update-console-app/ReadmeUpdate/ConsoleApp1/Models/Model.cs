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

    public class DirectoryInformation
    {
        public String name;
        public List<DirectoryInformation> subDirs;
        public List<FileData> files;
        public List<Sample> samples;
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
    }
}
