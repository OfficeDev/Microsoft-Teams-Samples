namespace MEAISentimentAnalysis
{
    public class ConfigOptions
    {
        public TeamsConfigOptions Teams { get; set; }
    }

    public class TeamsConfigOptions
    {
        public string BotType { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
        public string ApiKey { get; set; }
        public string ModelName { get; set; }
        public string ApplicationBaseUrl { get; set; }
    }
}