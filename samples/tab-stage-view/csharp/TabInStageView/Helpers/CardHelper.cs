// <copyright file="CardHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabInStageView.Helpers
{
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using System;
    using System.Collections.Generic;
    using TabInStageView.Models;

    /// <summary>
    /// Class that handles the card helper methods.
    /// </summary>
    public static class CardHelper
    {
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
                        Text = "Click the button to open the Url in tab stage view",
                        Weight = AdaptiveTextWeight.Bolder,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "view via card action",
                        Data = new AdaptiveCardAction
                        {
                            MsteamsCardAction = new CardAction
                            {
                                Type = "invoke",
                                Value = new TabInfoAction
                                {
                                    Type = "tab/tabInfoAction",
                                    TabInfo = new TabInfo
                                    {
                                        ContentUrl = "https://2f8fec398a1e.ngrok.io/tab",
                                        WebsiteUrl = "https://2f8fec398a1e.ngrok.io/tab",
                                        Name = "Tasks",
                                        EntityId = "entityId"
                                    }
                                }
                            },
                        },
                    },
                    new AdaptiveOpenUrlAction
                    {
                        Title = "view via deeplink",
                        Url = new Uri("https://teams.microsoft.com/l/stage/8749ff03-ae9e-4d33-88d8-bc9761ea7335/0?https://teams.microsoft.com/l/stage/8749ff03-ae9e-4d33-88d8-bc9761ea7335/0?context%3D%7B%E2%80%9CcontentUrl%E2%80%9D%3A%E2%80%9Dhttps%3A%2F%2F2f8fec398a1e.ngrok.io%2Ftab%E2%80%9D%2C%E2%80%9CwebsiteUrl%E2%80%9D%3A%E2%80%9Dhttps%3A%2F%2F2f8fec398a1e.ngrok.io%2Ftab%E2%80%9D%2C%E2%80%9Cname%E2%80%9D%3A%E2%80%9DContoso%E2%80%9D%7D"),
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