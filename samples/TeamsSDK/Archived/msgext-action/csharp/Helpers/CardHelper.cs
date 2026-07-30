using AdaptiveCards;
using Microsoft.Bot.Schema.Teams;
using Microsoft.BotBuilderSamples.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.BotBuilderSamples.Helpers
{
    /// <summary>
    /// Helper class for creating adaptive card attachments.
    /// </summary>
    public static class CardHelper
    {
        /// <summary>
        /// Creates an adaptive card attachment.
        /// </summary>
        /// <param name="action">The messaging extension action.</param>
        /// <param name="createCardResponse">The card response data.</param>
        /// <returns>A list of messaging extension attachments.</returns>
        public static List<MessagingExtensionAttachment> CreateAdaptiveCardAttachment(MessagingExtensionAction action, CardResponse createCardResponse)
        {
            var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                    {
                        CreateAdaptiveColumnSet("Name :", createCardResponse.Title),
                        CreateAdaptiveColumnSet("Designation :", createCardResponse.Subtitle),
                        CreateAdaptiveColumnSet("Description :", createCardResponse.Text)
                    }
            };

            return new List<MessagingExtensionAttachment>
                {
                    new MessagingExtensionAttachment
                    {
                        Content = JsonConvert.DeserializeObject(adaptiveCard.ToJson()),
                        ContentType = AdaptiveCard.ContentType
                    }
                };
        }

        /// <summary>
        /// Creates an adaptive card attachment for HTML.
        /// </summary>
        /// <param name="action">The messaging extension action.</param>
        /// <param name="createCardResponse">The card response data.</param>
        /// <returns>A list of messaging extension attachments.</returns>
        public static List<MessagingExtensionAttachment> CreateAdaptiveCardAttachmentForHtml(MessagingExtensionAction action, CardResponse createCardResponse)
        {
            var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                    {
                        CreateAdaptiveColumnSet("User Name :", createCardResponse.UserName),
                        CreateAdaptiveColumnSet("Password is :", createCardResponse.UserPwd)
                    }
            };

            return new List<MessagingExtensionAttachment>
                {
                    new MessagingExtensionAttachment
                    {
                        Content = JsonConvert.DeserializeObject(adaptiveCard.ToJson()),
                        ContentType = AdaptiveCard.ContentType
                    }
                };
        }

        /// <summary>
        /// Creates an adaptive column set.
        /// </summary>
        /// <param name="label">The label text.</param>
        /// <param name="value">The value text.</param>
        /// <returns>An adaptive column set.</returns>
        private static AdaptiveColumnSet CreateAdaptiveColumnSet(string label, string value)
        {
            return new AdaptiveColumnSet
            {
                Columns = new List<AdaptiveColumn>
                    {
                        new AdaptiveColumn
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = label,
                                    Wrap = true,
                                    Size = AdaptiveTextSize.Medium,
                                    Weight = AdaptiveTextWeight.Bolder
                                }
                            },
                            Width = AdaptiveColumnWidth.Auto
                        },
                        new AdaptiveColumn
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = value,
                                    Wrap = true,
                                    Size = AdaptiveTextSize.Medium
                                }
                            },
                            Width = AdaptiveColumnWidth.Auto
                        }
                    }
            };
        }
    }
}
