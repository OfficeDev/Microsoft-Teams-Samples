using System;

namespace Microsoft.BotBuilderSamples.SPListBot.Models
{
    public class ConversationData
    {
        public string odatametadata { get; set; }
        public Values[] value { get; set; }
    }

    public class Values
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public object ServerRedirectedEmbedUri { get; set; }
        public string ServerRedirectedEmbedUrl { get; set; }
        public string ContentTypeId { get; set; }
        public string Title { get; set; }
        public int AuthorId { get; set; }
        public DateTime Modified { get; set; }
        public int EditorId { get; set; }
        public string OData__UIVersionString { get; set; }
        public string GUID { get; set; }
    }
}
