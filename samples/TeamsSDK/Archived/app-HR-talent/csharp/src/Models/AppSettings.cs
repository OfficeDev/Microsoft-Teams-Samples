namespace TeamsTalentMgmtApp.Models
{
    public class AppSettings
    {
        private string _baseUrl = "";

        public string BaseUrl { get => _baseUrl; set => _baseUrl = value.TrimEnd('/') + "/"; }

        public string TeamsAppId { get; set; }
        public string MicrosoftAppId { get; set; }
        public string MicrosoftDirectoryId { get; set; }
        public string MicrosoftAppPassword { get; set; }
        public string OAuthConnectionName { get; set; }
        public string ApplicationIdUri { get; set; }
        public string ServiceUrl { get; set; }
        public string OpenPositionsTabEntityId { get; set; }
    }
}
