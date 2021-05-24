using Newtonsoft.Json;

namespace Microsoft.Teams.Apps.VirtualAssistant.Models
{
    // Skill Card action data should contain skillId parameter
    // This class is used to deserialize it and get skillId 
    public class SkillCardActionData
    {
        /// <summary>
        /// Gets skillId.
        /// </summary>
        [JsonProperty("skillId")]
        public string SkillId { get; set; }
    }
}
