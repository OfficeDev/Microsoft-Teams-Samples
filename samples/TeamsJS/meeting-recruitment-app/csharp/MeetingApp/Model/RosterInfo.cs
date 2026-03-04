using Newtonsoft.Json;

// Model for roster information.
namespace MeetingApp.Model
{
    /// <summary>
    /// Class with properties to store Roster and note(Added during share asset) information
    /// </summary>
    public class ConversationData
    {
        public RosterInfo[] Roster { get; set; }

        public string Note { get; set; }

        public string SharedByName { get; set; }
    }

    /// <summary>
    /// Class with properties related to Roster info
    /// </summary>
    public class RosterInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("aadObjectId")]
        public string AadObjectId { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("objectId")]
        public string ObjectId { get; set; }

        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        [JsonProperty("surname")]
        public string Surname { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("userPrincipalName")]
        public string UserPrincipalName { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        [JsonProperty("userRole")]
        public string UserRole { get; set; }
    }
}
