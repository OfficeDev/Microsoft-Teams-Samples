// <copyright file="CardHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabWithAdpativeCardFlow.Helpers
{
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using System;
    using System.Collections.Generic;
    using TabWithAdpativeCardFlow.Models;

    /// <summary>
    /// Class that handles the card helper methods.
    /// </summary>
    public static class CardHelper
    {
        /// <summary>
        /// Sample Adaptive card to show in tab.
        /// </summary>
        public static AdaptiveCard GetSampleAdaptiveCard1(string profilePhoto, string displayName)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                     new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri(profilePhoto),
                                        Style = AdaptiveImageStyle.Person,
                                        Size = AdaptiveImageSize.Small,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Stretch,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = "Hello " + displayName,
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Wrap = true,
                                    },
                                },
                            },
                        },
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Show Task Module",
                        Data = new AdaptiveCardAction
                        {
                            MsteamsCardAction = new CardAction
                            {
                                Type = "task/fetch",
                            },
                        },
                    },
                },
            };

            return card;
        }

        /// <summary>
        /// Sample Adaptive card to show in tab.
        /// </summary>
        public static AdaptiveCard GetSampleAdaptiveCard3()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                     new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = "Enter the ID of youtube video",
                                        Wrap = true,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Stretch,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextInput
                                    {
                                        Id = "youTubeVideoId",
                                        Value = "jugBQqE_2sM",
                                    },
                                },
                            },
                        },
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Update",
                        Data = new AdaptiveCardAction
                        {
                            MsteamsCardAction = new CardAction
                            {
                                Type = "task/fetch",
                            },
                            Id="youTubeVideo"
                        },
                    },
                },
            };

            return card;
        }

        /// <summary>
        /// Sample Adaptive card to show in tab.
        /// </summary>
        public static AdaptiveCard GetSampleAdaptiveCard2()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveImage
                    {
                        Url = new Uri("https://cdn.vox-cdn.com/thumbor/Ndb49Uk3hjiquS041NDD0tPDPAs=/0x169:1423x914/fit-in/1200x630/cdn.vox-cdn.com/uploads/chorus_asset/file/7342855/microsoftteams.0.jpg"),
                        AltText = "AlternativeText",
                        PixelHeight = 300,
                        PixelWidth = 400,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "'tab/fetch' is the first invoke request that your bot receives when a user opens an Adaptive Card tab. When your bot receives the request, it either sends a tab continue response or a tab auth response.",
                        Wrap = true,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "'tab/submit' request is triggered to your bot with the corresponding data through the Action.Submit function of Adaptive Card.",
                        Spacing = AdaptiveSpacing.Medium,
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Sign out",
                    },
                },
            };

            return card;
        }

        /// <summary>
        /// Sample Adaptive card for Task module.
        /// </summary>
        public static Attachment GetAdaptiveCardForTaskModule()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Sample task module flow for tab",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                    new AdaptiveImage
                    {
                        Url = new Uri("https://cdn.vox-cdn.com/thumbor/Ndb49Uk3hjiquS041NDD0tPDPAs=/0x169:1423x914/fit-in/1200x630/cdn.vox-cdn.com/uploads/chorus_asset/file/7342855/microsoftteams.0.jpg"),
                        AltText = "AlternativeText",
                        PixelHeight = 80,
                        PixelWidth = 80,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Close",
                        Data = new AdaptiveCardAction
                        {
                            MsteamsCardAction = new CardAction
                            {
                                Type = "task/submit",
                            },
                        },
                    },
                },
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Sample Adaptive card for sign out success message.
        /// </summary>
        public static AdaptiveCard GetSignOutCard()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Sign out successful. Please refresh to Sign in again.",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                },
            };

            return card;
        }

        /// <summary>
        /// Sample Adaptive card task module submit.
        /// </summary>
        public static Attachment GetTaskModuleSubmitCard()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "The action called task/submit",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                },
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}