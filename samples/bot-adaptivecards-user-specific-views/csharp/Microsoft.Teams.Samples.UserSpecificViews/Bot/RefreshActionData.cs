namespace Microsoft.Teams.Samples.UserSpecificViews.Bot
{
    /// <summary>
    /// Refresh action data.
    /// </summary>
    public class RefreshActionData
    {
        public Action action { get; set; }
        public string trigger { get; set; }
    }

    /// <summary>
    /// Action model class.
    /// </summary>
    public class Action
    {
        public string type { get; set; }
        public string title { get; set; }
        public Data data { get; set; }
        public string verb { get; set; }
    }

    /// <summary>
    /// Data model class.
    /// </summary>
    public class Data
    {
        public int RefreshCount { get; set; }

        public string CardType { get; set; }
    }
}
