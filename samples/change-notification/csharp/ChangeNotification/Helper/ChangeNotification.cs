using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using AdaptiveCards;
using Microsoft.Extensions.Caching.Memory;

namespace ChangeNotification.Helper
{
    public class ChangeNotificationHelper
    {

        public Microsoft.Bot.Schema.Attachment ShowAdaptiveCard(string Header, ChangeNotification.Model.ResourceData InfoData)
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
