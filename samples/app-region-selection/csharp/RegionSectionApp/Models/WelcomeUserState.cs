using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Models
{
    /// <summary>
    /// Represents the state of the user in the welcome conversation.
    /// </summary>
    public class WelcomeUserState
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user has selected a domain.
        /// </summary>
        public bool DidUserSelectDomain { get; set; } = false;

        /// <summary>
        /// Gets or sets the selected domain.
        /// </summary>
        public string SelectedDomain { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the selected region.
        /// </summary>
        public string SelectedRegion { get; set; } = string.Empty;
    }
}
