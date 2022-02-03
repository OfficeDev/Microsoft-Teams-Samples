using Microsoft.Azure.Cosmos.Table;

namespace IncomingWebhook.Data.Models
{
    /// <summary>
    /// Class for Notes related properties.
    /// </summary>
    public class ResponseEntity: TableEntity
    {
        public string Comment { get; set; }
    }
}
