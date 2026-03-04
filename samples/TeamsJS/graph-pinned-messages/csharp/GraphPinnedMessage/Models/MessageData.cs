using Microsoft.Graph;

namespace GraphPinnedMessage.Models
{
    public class MessageData
    {
        public string Id { get; set; }

        public string Message { get; set; }

        public List<PinnedMessage> Messages { get; set; }
    }
}
