namespace ReleaseManagement.Services
{
    using System.IO;
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    /// <summary>
    /// Creates Adaptive Card attachments.
    /// </summary>
    public class CardFactory : ICardFactory
    {
        /// <inheritdoc/>
        public Attachment CreateAdaptiveCardAttachement(string filePath, object dataObj)
        {
            var cardJson = File.ReadAllText(filePath);
            if (dataObj != null)
            {
                var template = new AdaptiveCardTemplate(cardJson);
                cardJson = template.Expand(dataObj);
            }

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(cardJson),
            };
        }
    }
}
