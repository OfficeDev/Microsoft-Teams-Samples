namespace ProactiveBot.Models
{
    public class AppModel
    {
        public string AppId { get; set; }
        public string AppName { get; set; }
        public string AppDesc { get; set; }
        public string AppDistributionMethod { get; set; }
        public string Id { get; set; }
    }

    public class CheckCount
    {
        public int Exist_Count { get; set; }

        public int New_Count { get; set; }
    }

    public class CheckAppStatus
    {
        public int AppCount { get; set; }

        public bool CheckStatus { get; set; }
    }
}