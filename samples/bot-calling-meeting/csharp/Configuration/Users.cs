using System.Collections.Generic;

namespace CallingBotSample.Configuration
{
    public class Users
    {
        public List<User> users { get; set; }
    }

    public class User
    {
        public string DisplayName { get; set; }
        public string Id { get; set; }
    }

}
