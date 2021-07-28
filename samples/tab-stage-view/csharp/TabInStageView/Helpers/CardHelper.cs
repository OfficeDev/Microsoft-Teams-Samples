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
                        Title = "Invoke stage view via Adaptive card action",
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
                                        ContentUrl = "https://microsoft.sharepoint.com/teams/LokisSandbox/SitePages/Sandbox-Page.aspx",
                                        WebsiteUrl = "https://microsoft.sharepoint.com/teams/LokisSandbox/SitePages/Sandbox-Page.aspx",
                                        Name = "Tasks",
                                        EntityId = "entityId"
                                    }
                                }
                            },
                        },
                    },
                    new AdaptiveOpenUrlAction
                    {
                        Title = "Invoke stage view via deeplink",
                        Url = new Uri("https://teams.microsoft.com/l/stage/2a527703-1f6f-4559-a332-d8a7d288cd88/0?context={“contentUrl”:https%3A%2F%2Fmicrosoft.sharepoint.com%2Fteams%2FLokisSandbox%2FSitePages%2FSandbox-Page.aspx”,“websiteUrl”:”https%3A%2F%2Fmicrosoft.sharepoint.com%2Fteams%2FLokisSandbox%2FSitePages%2FSandbox-Page.aspx”,“name”:”Contoso”}"),
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