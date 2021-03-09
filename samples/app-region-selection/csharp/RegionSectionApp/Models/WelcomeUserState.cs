using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Models
{
    public class WelcomeUserState
    {
        // Gets or sets whether the user has been welcomed in the conversation.
        public bool DidUserSelectedDomain { get; set; } = false;
        public string SelectedDomain { get; set; } = string.Empty;
        public string SelectedRegion { get; set; } = string.Empty;
    }
}
