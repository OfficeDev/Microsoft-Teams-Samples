using Microsoft.Graph;
using Newtonsoft.Json;

namespace TabActivityFeed.Model
{
    /// <summary>
    ///  The type TeamMembersNotificationRecipient.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class TeamMembersNotificationRecipient : TeamworkNotificationRecipient
    {
        /// <summary>
        /// Gets or sets teamId.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "teamId", Required = Required.Default)]
        public string TeamId { get; set; }

        /// <summary>
        /// Initializes a new instance of the Microsoft.Graph.teamMembersNotificationRecipient class.
        /// </summary>
        public TeamMembersNotificationRecipient()
        {
            ODataType = "microsoft.graph.teamMembersNotificationRecipient";
        }
    }
}