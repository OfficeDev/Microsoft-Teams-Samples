namespace SidePanel.Models
{
    // Organizer role response model
    public class UserMeetingRoleServiceResponse
    {
        public User? user { get; set; }
        public Meeting? meeting { get; set; }
        public Conversation? conversation { get; set; }
    }

    public class User
    {
        public string id { get; set; } = string.Empty;
        public string aadObjectId { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string givenName { get; set; } = string.Empty;
        public string surname { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string userPrincipalName { get; set; } = string.Empty;
        public string tenantId { get; set; } = string.Empty;
        public string userRole { get; set; } = string.Empty;
    }

    public class Meeting
    {
        public string role { get; set; } = string.Empty;
        public bool inMeeting { get; set; }
    }

    public class Conversation
    {
        public bool isGroup { get; set; }
        public string id { get; set; } = string.Empty;
    }
}
