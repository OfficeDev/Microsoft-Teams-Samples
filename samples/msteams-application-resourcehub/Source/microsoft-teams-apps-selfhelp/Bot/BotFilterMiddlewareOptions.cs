namespace Microsoft.Teams.Selfhelp.Authentication.Bot
{
    /// <summary>
    /// Options used for creating the bot filter middleware.
    /// </summary>
    public class BotFilterMiddlewareOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotFilterMiddlewareOptions"/> class.
        /// </summary>
        public BotFilterMiddlewareOptions()
        {
            // Default this value to false.
            this.DisableTenantFilter = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the tenant
        /// filtering for the bot should be disabled.
        /// </summary>
        public bool DisableTenantFilter { get; set; }

        /// <summary>
        /// Gets or sets the allowed tenants list.
        /// </summary>
        public string AllowedTenants { get; set; }
    }
}