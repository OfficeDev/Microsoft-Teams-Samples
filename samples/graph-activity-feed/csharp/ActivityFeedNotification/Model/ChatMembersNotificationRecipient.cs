using Microsoft.Graph;
using Newtonsoft.Json;

namespace TabActivityFeed.Model
{
    /// <summary>
    /// The type ChatMembersNotificationRecipient.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ChatMembersNotificationRecipient : TeamworkNotificationRecipient
    {
        /// <summary>
        /// Gets or sets chatId.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "chatId", Required = Required.Default)]
        public string ChatId { get; set; }

        /// <summary>
        /// Initializes a new instance of the Microsoft.Graph.chatMembersNotificationRecipient class.
        /// </summary>
        public ChatMembersNotificationRecipient()
        {
            ODataType = "microsoft.graph.chatMembersNotificationRecipient";
        }
    }
}