namespace meetings_transcription
{
    public class ConfigOptions
    {
        public TeamsConfigOptions Teams { get; set; }
        public AzureConfigOptions Azure { get; set; }
    }

    public class TeamsConfigOptions
    {
        public string BotType { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
    }

    public class AzureConfigOptions
    {
        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppPassword { get; set; }
        public string MicrosoftAppTenantId { get; set; }
        public string AppBaseUrl { get; set; }
        public string UserId { get; set; }
        public string GraphApiEndpoint { get; set; }
    }
}