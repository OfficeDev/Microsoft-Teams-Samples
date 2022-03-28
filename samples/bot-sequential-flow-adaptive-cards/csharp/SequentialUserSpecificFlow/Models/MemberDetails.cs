namespace SequentialUserSpecificFlow.Models
{
    public class MemberDetails
    {
        public Info info { get; set; }
    }
    public class Info
    {
        public string value { get; set; }
        public string title { get; set; }
    }

    public class IsBotInstalled
    {
        public bool isBotInstalled { get; set; }
    }
}
