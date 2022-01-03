using Microsoft.Azure.Cosmos.Table;

namespace MeetingApp.Data.Models
{
    /// <summary>
    /// Class for Notes related properties.
    /// </summary>
    public class CardEntity: TableEntity
    {
        public string WebhookUrl { get; set; }

        public string CardBody { get; set; }
    }
}
