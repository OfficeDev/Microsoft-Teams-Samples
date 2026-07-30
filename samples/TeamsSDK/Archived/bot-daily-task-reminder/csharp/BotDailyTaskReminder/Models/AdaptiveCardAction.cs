// <copyright file="AdaptiveCardAction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace BotDailyTaskReminder.Models
{
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    /// <summary>
    /// Adaptive card action model class.
    /// </summary>
    public class AdaptiveCardAction
    {
        /// <summary>
        /// Gets or sets Ms Teams card action type.
        /// </summary>
        [JsonProperty("msteams")]
        public CardAction MsteamsCardAction { get; set; }

        /// <summary>
        /// Gets or sets id value of turncontext activity.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}