namespace SidePanel
{
    public class ConfigOptions
    {
        public TeamsConfigOptions Teams { get; set; } = new();
        public string BaseUrl { get; set; } = string.Empty;
    }

    public class TeamsConfigOptions
    {
        public string BotType { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
    }
}