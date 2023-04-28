namespace MeetingApp.Model
{
    /// <summary>
    /// Class with properties related to ShareAsset feature
    /// </summary>
    public class AssetsDetails
    {
        public string Message { get; set; }

        public string SharedBy { get; set; }

        public string MeetingId { get; set; }

        public string[] Files { get; set; }
    }
}
