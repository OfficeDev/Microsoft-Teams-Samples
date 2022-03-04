// <copyright file="CardFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.TeamsServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Card factory implementation.
    /// </summary>
    internal class CardFactory : ICardFactory
    {
        /// <summary>
        /// Stage view url format.
        ///
        /// {0} - App id.
        /// {1} - Context.
        /// </summary>
        private const string StageViewUrlFormat = "https://teams.microsoft.com/l/stage/{0}/0?context={1}";
        private const string ReviewResourceCardTemplatePath = "{0}\\assets\\templates\\review-resource.json";
        private readonly IAppSettings appSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardFactory"/> class.
        /// </summary>
        /// <param name="appSettings">App settings.</param>
        public CardFactory(IAppSettings appSettings)
        {
            this.appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        /// <inheritdoc/>
        public Attachment GetResourceContentCard(Resource resource)
        {
            var data = new
            {
                resource = new
                {
                    id = resource.Id,
                    url = resource.Url,
                    name = resource.Name,
                    imageUrl = resource.PreviewImageUrl,
                    stageUrl = this.GetStageViewUrl(resource),
                },
            };

            var template = this.GetCardTemplate(ReviewResourceCardTemplatePath);
            var serializedJson = template.Expand(data);
            return this.CreateAttachment(serializedJson);
        }

        /// <inheritdoc/>
        public Attachment GetResourcePreviewCard(Resource resource)
        {
            var card = new HeroCard
            {
                Title = resource.Name,
                Text = resource.Url,
                Images = new List<CardImage> { new CardImage(resource.PreviewImageUrl) },
                Buttons = new List<CardAction>() { },
            };

            return card.ToAttachment();
        }

        private string GetStageViewUrl(Resource resource)
        {
            var contextJsonObject = JObject.FromObject(new
            {
                contentUrl = resource.Url,
                websiteUrl = resource.Url,
                name = resource.Name,
            });
            var serializedContext = JsonConvert.SerializeObject(contextJsonObject, Formatting.None);
            var stageViewLink = string.Format(StageViewUrlFormat, this.appSettings.CatalogAppId, Uri.EscapeDataString(serializedContext));
            return stageViewLink;
        }

        private AdaptiveCardTemplate GetCardTemplate(string templatePath)
        {
            templatePath = string.Format(templatePath, AppDomain.CurrentDomain.BaseDirectory);
            return new AdaptiveCardTemplate(File.ReadAllText(templatePath));
        }

        private Attachment CreateAttachment(string adaptiveCardJson)
        {
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard.Card,
            };
        }
    }
}
