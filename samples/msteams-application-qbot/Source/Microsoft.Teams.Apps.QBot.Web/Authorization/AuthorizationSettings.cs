namespace Microsoft.Teams.Apps.QBot.Web.Authorization
{
    using System.Collections.Generic;

    /// <summary>
    /// Authorization settings.
    /// </summary>
    public class AuthorizationSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationSettings"/> class.
        /// </summary>
        /// <param name="adminUpnList">Admin upn list.</param>
        public AuthorizationSettings(ISet<string> adminUpnList)
        {
            this.AdminUpnList = adminUpnList ?? throw new System.ArgumentNullException(nameof(adminUpnList));
        }

        /// <summary>
        /// Gets admin upn list.
        /// </summary>
        public ISet<string> AdminUpnList { get; }
    }
}
