namespace Microsoft.Teams.Apps.QBot.Web.SpaHost
{
    /// <summary>
    /// The SPA Host Configuration is the configuration of the QBot single-page app
    /// </summary>
    public sealed class SpaHostConfiguration
    {
        /// <summary>
        /// Gets or sets the Application Insights (AI) instrumentation key for the client
        /// </summary>
        /// <value>The instrumentation key.</value>
        public string ApplicationInsightsInstrumentationKey { get; set; }

        /// <summary>
        /// Gets or sets the Bot's app name
        /// </summary>
        /// <value>The Teams' bot name.</value>
        public string BotAppName { get; set; }
    }
}
