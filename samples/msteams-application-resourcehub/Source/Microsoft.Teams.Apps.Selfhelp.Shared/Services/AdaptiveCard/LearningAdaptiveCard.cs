namespace Microsoft.Teams.Apps.Selfhelp.Shared.Services.AdaptiveCard
{
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;

    public class LearningAdaptiveCard
    {
        public string CardTitle { get; set; }

        public string ProfileUrl { get; set; }

        public string ProfileName { get; set; }

        public string CardTime { get; set; }

        public string CardDate { get; set; }

        public string LearningContent { get; set; }

        public string LearningContentUrl { get; set; }

        public string CardMessage { get; set; }

        public string LearningId { get; set; }

        public ItemType ItemType { get; set; }

        public string AppId { get; set; }

        public string BaseUrl { get; set; }
    }
}