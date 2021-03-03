using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.SpfxBot.Models
{
    public class UserProfile
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public bool PromptedUserForName { get; set; }
    }
}
