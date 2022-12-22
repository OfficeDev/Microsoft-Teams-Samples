using Newtonsoft.Json;

namespace Microsoft.Graph
{
    //
    // Summary:
    //     The type TeamMembersNotificationRecipient.
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class TeamMembersNotificationRecipient : TeamworkNotificationRecipient
    {
        //
        // Summary:
        //     Gets or sets teamId.
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "teamId", Required = Required.Default)]
        public string TeamId { get; set; }

        //
        // Summary:
        //     Initializes a new instance of the Microsoft.Graph.teamMembersNotificationRecipient
        //     class.
        public TeamMembersNotificationRecipient()
        {
            base.ODataType = "microsoft.graph.teamMembersNotificationRecipient";
        }
    }
}