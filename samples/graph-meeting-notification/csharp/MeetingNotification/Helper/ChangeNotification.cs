using AdaptiveCards;
using System.Collections.Generic;

namespace MeetingNotification.Helper
{
    public class ChangeNotificationHelper
    {
        public Microsoft.Bot.Schema.Attachment GetAvailabilityChangeCard(string Header, MeetingNotification.Model.ResourceData InfoData)
        {
            AdaptiveCard adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            adaptiveCard.Body = new List<AdaptiveElement>()
            {
                new AdaptiveTextBlock(){Text=Header, Weight=AdaptiveTextWeight.Bolder}
            };

            AdaptiveTextBlock textBlock = new AdaptiveTextBlock()
            {
                Text = "- " + "Availability : " + InfoData.Availability + " \r" +
                        "- " + "Activity : " + InfoData.Activity + " \r",
            };
            adaptiveCard.Body.Add(textBlock);

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };
        }
    }
}