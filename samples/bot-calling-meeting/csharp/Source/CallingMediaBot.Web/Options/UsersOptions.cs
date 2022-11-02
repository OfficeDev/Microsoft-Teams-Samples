namespace CallingMediaBot.Web.Options
{
    public class UsersOptions
    {
        public UserOptions[] users { get; set; }
    }

    public class UserOptions
    {
        public string DisplayName { get; set; }
        public string Id { get; set; }
    }
}
