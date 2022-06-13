namespace ReleaseManagement.Services
{
    using System.IO;
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    /// <summary>
    /// Returns CardAttachment.
    /// </summary>
    public class CardFactory : ICardFactory
    {
        /// <inheritdoc/>
        public Attachment CreateAdaptiveCardAttachement(string filePath, object dataObj)
        {
            var cardJSON = File.ReadAllText(filePath);
            if (dataObj != null)
            {
                AdaptiveCardTemplate template = new AdaptiveCardTemplate(cardJSON);
                cardJSON = template.Expand(dataObj);
            }

            var adaptiveCardAttachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(cardJSON),
            };

            return adaptiveCardAttachment;
        }
    }
}
