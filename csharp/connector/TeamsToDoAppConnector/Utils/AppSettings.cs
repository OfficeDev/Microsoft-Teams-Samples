using System.Configuration;

namespace TeamsToDoAppConnector.Utils
{
    /// <summary>
    /// Represents a class to strore settings for easy access.
    /// </summary>
    public static class AppSettings
    {
        public static readonly string BaseUrl;
        public static readonly string ConnectorAppId;

        static AppSettings()
        {
            BaseUrl = ConfigurationManager.AppSettings["BaseUrl"];
            ConnectorAppId = ConfigurationManager.AppSettings["ConnectorAppId"];
        }
    }
}