namespace Microsoft.Teams.Apps.Selfhelp.Shared.Services.AdaptiveCard
{
    public class SendNotificationCards
    {
        public string CardTitle { get; set; }

        public List<checkBoxEntity> learningEntity { get; set; }

        public int Count { get; set; }

        public string KnowMoreLink { get; set; }
    }

    public class checkBoxEntity
    {
        public string Title { get; set; }

        public string ItemType { get; set; }

        public string TileImageLink { get; set; }

        public string RowKey { get; set; }
    }
}