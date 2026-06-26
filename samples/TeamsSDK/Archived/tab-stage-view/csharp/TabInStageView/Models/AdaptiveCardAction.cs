// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TabInStageView.Models
{
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    /// <summary>
    /// Adaptive card action model class.
    /// </summary>
    public class AdaptiveCardAction
    {
        /// <summary>
        /// Gets or sets Ms Teams card action.
        /// </summary>
        [JsonProperty("msteams")]
        public required CardAction MsteamsCardAction { get; set; }
    }
}