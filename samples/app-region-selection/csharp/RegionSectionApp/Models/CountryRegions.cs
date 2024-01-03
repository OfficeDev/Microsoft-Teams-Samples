namespace Microsoft.BotBuilderSamples.Models
{
    public class Rootobject
    {
        public Regiondomain[] regionDomains { get; set; }
    }

    public class Regiondomain
    {
        /// <summary>
        /// Regiondomain
        /// </summary>
        public int id { get; set; }
        public string region { get; set; }
        public string country { get; set; }
        public string domain { get; set; }
    }
}
