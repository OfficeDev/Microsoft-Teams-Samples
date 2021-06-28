// <copyright file="CardHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace CallingBotSample.Helpers
{
    using CallingBotSample.Interfaces;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System.IO;

    /// <summary>
    /// Helper for cards.
    /// </summary>
    public class CardHelper : ICard
    {
        private readonly ILogger<CardHelper> logger;

        public CardHelper(ILogger<CardHelper> logger)
        {
            this.logger = logger;
        }

        public Attachment GetWelcomeCardAttachment()
        {
            var welcomeCardAttachment = new Attachment();
            try
            {
                string[] welcomeCardPaths = { ".", "Resources", "WelcomeCard.json" };
                var welcomeCardString = File.ReadAllText(Path.Combine(welcomeCardPaths));
                welcomeCardAttachment.ContentType = "application/vnd.microsoft.card.adaptive";
                welcomeCardAttachment.Content = JsonConvert.DeserializeObject(welcomeCardString);
            }
            catch (System.Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
            }

            return welcomeCardAttachment;
        }
    }
}