namespace SidePanel.Models
{
    //Organizer role response model

    public class UserMeetingRoleServiceResponse
    {
        public User user { get; set; }
        public Meeting meeting { get; set; }
        public Conversation conversation { get; set; }
    }

    public class User
    {
        public string id { get; set; }
        public string aadObjectId { get; set; }
        public string name { get; set; }
        public string givenName { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string userPrincipalName { get; set; }
        public string tenantId { get; set; }
        public string userRole { get; set; }
    }

    public class Meeting
    {
        public string role { get; set; }
        public bool inMeeting { get; set; }
    }

    public class Conversation
    {
        public bool isGroup { get; set; }
        public string id { get; set; }
    }

}