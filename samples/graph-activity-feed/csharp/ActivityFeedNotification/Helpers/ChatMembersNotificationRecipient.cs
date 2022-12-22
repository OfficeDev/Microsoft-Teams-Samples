using Newtonsoft.Json;

namespace Microsoft.Graph
{
    //
    // Summary:
    //     The type ChatMembersNotificationRecipient.
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ChatMembersNotificationRecipient : TeamworkNotificationRecipient
    {
        //
        // Summary:
        //     Gets or sets chatId.
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "chatId", Required = Required.Default)]
        public string ChatId { get; set; }

        //
        // Summary:
        //     Initializes a new instance of the Microsoft.Graph.chatMembersNotificationRecipient
        //     class.
        public ChatMembersNotificationRecipient()
        {
            base.ODataType = "microsoft.graph.chatMembersNotificationRecipient";
        }
    }
}