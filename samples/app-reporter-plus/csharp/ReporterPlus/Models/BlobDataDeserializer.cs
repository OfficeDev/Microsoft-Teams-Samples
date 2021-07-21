using System.Collections.Generic;

namespace ReporterPlus.Models
{
    public class BlobDataDeserializer
    {
        public string requestID { get; set; }
        public string status { get; set; }
        public string itemName { get; set; }
        public string itemCode { get; set; }
        public string assignedToName { get; set; }
        public string assignedToId { get; set; }
        public string assignedToMail { get; set; }
        public string submittedByName { get; set; }
        public string submittedById { get; set; }
        public string submittedByMail { get; set; }
        public string assignedToUserImage { get; set; }
        public string submittedByUserImage { get; set; }
        public List<Images> imageURL { get; set; }
        public string audioURL { get; set; }
        public string comments { get; set; }
        public string conversationId { get; set; }
        public string messageId { get; set; }
    }

    public class Images
    {
        public string url { get; set; }
    }
}
