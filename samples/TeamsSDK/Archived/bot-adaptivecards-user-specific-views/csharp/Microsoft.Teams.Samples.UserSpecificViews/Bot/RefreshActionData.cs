namespace Microsoft.Teams.Samples.UserSpecificViews.Bot
{
    /// <summary>
    /// Refresh action data.
    /// </summary>
    public class RefreshActionData
    {
        public Action action { get; set; } = new();
        public string trigger { get; set; } = string.Empty;
    }

    /// <summary>
    /// Action model class.
    /// </summary>
    public class Action
    {
        public string type { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public Data data { get; set; } = new();
        public string verb { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data model class.
    /// </summary>
    public class Data
    {
        public int RefreshCount { get; set; }
        public string CardType { get; set; } = string.Empty;
    }
}
