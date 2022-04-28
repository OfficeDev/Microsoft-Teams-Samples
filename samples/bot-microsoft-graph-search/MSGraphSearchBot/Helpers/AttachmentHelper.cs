using AdaptiveCards;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MSGraphSearchSample.Helpers
{
    public static class AttachmentHelper
    {
        public static Attachment GetAttachment(string jsonContent)
        {
            return new Attachment() { Content = JsonConvert.DeserializeObject(jsonContent), ContentType = AdaptiveCard.ContentType };
        }

        public static Attachment BuildAttachment(List<AdaptiveElement> elements)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 4));
            card.Body.AddRange(elements);
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }
    }
}
