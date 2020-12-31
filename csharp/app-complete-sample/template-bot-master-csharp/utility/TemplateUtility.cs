using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Utility
{
    /// <summary>
    /// Get the locale from incoming activity payload and handle compose extension methods
    /// </summary>
    public static class TemplateUtility
    {
        public static string GetLocale(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            //Get the locale from activity
            if (activity.Entities != null)
            {
                foreach(var entity in activity.Entities)
                {
                    if (string.Equals(entity.Type.ToString().ToLower(), "clientinfo"))
                    {
                        var locale = entity.Properties["locale"];
                        if (locale != null)
                        {
                            return locale.ToString();
                        }
                    }
                }
            }
            return activity.Locale;
        }

        public static ComposeExtensionAttachment CreateComposeExtensionCardsAttachments(WikiHelperSearchResult wikiResult, string selectedType)
        {
            return GetComposeExtensionMainResultAttachment(wikiResult, selectedType).ToComposeExtensionAttachment(GetComposeExtensionPreviewAttachment(wikiResult, selectedType));
        }

        public static ComposeExtensionAttachment CreateComposeExtensionCardsAttachmentsSelectedItem(WikiHelperSearchResult wikiResult, string selectedType)
        {
            return GetComposeExtensionMainResultAttachment(wikiResult, selectedType).ToComposeExtensionAttachment();
        }

        public static Attachment GetComposeExtensionMainResultAttachment(WikiHelperSearchResult wikiResult, string selectedType)
        {
            CardType cardType;
            Attachment cardAttachment = null;

            var images = new List<CardImage>
            {
                new CardImage(wikiResult.imageUrl)
            };

            if (Enum.TryParse(selectedType, out cardType))
            {
                switch (cardType)
                {
                    case CardType.hero:
                        cardAttachment = new HeroCard()
                        {
                            Title = wikiResult.highlightedTitle,
                            Text = wikiResult.text,
                            Images = images
                        }.ToAttachment();
                        break;
                    case CardType.thumbnail:
                        cardAttachment = new ThumbnailCard()
                        {
                            Title = wikiResult.highlightedTitle,
                            Text = wikiResult.text,
                            Images = images
                        }.ToAttachment();
                        break;
                }
            }

            return cardAttachment;
        }

        public static Attachment GetComposeExtensionPreviewAttachment(WikiHelperSearchResult wikiResult, string selectedType)
        {
            string invokeVal = GetCardActionInvokeValue(wikiResult);
            var tapAction = new CardAction("invoke", value: invokeVal);

            CardType cardType;
            Attachment cardAttachment = null;

            var images = new List<CardImage>
            {
                new CardImage(wikiResult.imageUrl)
            };

            if (Enum.TryParse(selectedType, out cardType))
            {
                switch (cardType)
                {
                    case CardType.hero:
                        cardAttachment = new HeroCard()
                        {
                            Title = wikiResult.highlightedTitle,
                            Tap = tapAction,
                            Images = images
                        }.ToAttachment();
                        break;
                    case CardType.thumbnail:
                        cardAttachment = new ThumbnailCard()
                        {
                            Title = wikiResult.highlightedTitle,
                            Tap = tapAction,
                            Images = images
                        }.ToAttachment();
                        break;
                }
            }

            return cardAttachment;
        }

        private static string GetCardActionInvokeValue(WikiHelperSearchResult wikiResult)
        {
            InvokeValue invokeValue = new InvokeValue(wikiResult.imageUrl, wikiResult.text, wikiResult.highlightedTitle);
            return JsonConvert.SerializeObject(invokeValue);
        }

        /// <summary>
        /// Parse the invoke request json and returned the invoke value
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string ParseInvokeRequestJson(string inputString)
        {
            JObject invokeObjects = JObject.Parse(inputString);

            if (invokeObjects.Count > 0)
            {
                return invokeObjects[Strings.InvokeRequestJsonKey].Value<string>();
            }

            return null;
        }

        /// <summary>
        /// Parse the Update card  message back json value returned the updated counter
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static int ParseUpdateCounterJson(Activity activity)
        {
            if (activity != null && activity.Value != null)
            {
                JObject invokeObjects = activity.Value as JObject;

                if (invokeObjects != null && invokeObjects.Count > 0)
                {
                    return invokeObjects["updateKey"].Value<int>();
                }
            }

            return 0;
        }

        public static async Task<BotData> GetBotUserDataObject(IBotDataStore<BotData> botDataStore, Activity activity)
        {
            IAddress key = Address.FromActivity(activity);
            BotData botData = await botDataStore.LoadAsync(key, BotStoreType.BotUserData, CancellationToken.None);
            return botData;
        }

        public static async Task SaveBotUserDataObject(IBotDataStore<BotData> botDataStore, Activity activity, BotData userData)
        {
            IAddress key = Address.FromActivity(activity);
            await botDataStore.SaveAsync(key, BotStoreType.BotUserData, userData, CancellationToken.None);            
        }
    }

    public class InvokeValue
    {
        public string imageUrl { get; set; }
        public string text { get; set; }
        public string highlightedTitle { get; set; }

        public InvokeValue(string urlValue, string textValue, string highlightedTitleValue)
        {
            imageUrl = urlValue;
            text = textValue;
            highlightedTitle = highlightedTitleValue;
        }
    }
}