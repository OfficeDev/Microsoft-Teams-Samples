namespace Microsoft.BotBuilderSamples.Models
{
    /// <summary>
    /// Represents the root object containing region domains.
    /// </summary>
    public class RootObject
    {
        public RegionDomain[] RegionDomains { get; set; }
    }

    /// <summary>
    /// Represents a region domain with its associated properties.
    /// </summary>
    public class RegionDomain
    {
        public int Id { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string Domain { get; set; }
    }
}
