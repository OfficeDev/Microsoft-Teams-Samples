// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using AdaptiveCards;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Hosting.Internal;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Teams.Samples.TaskModule.Web.Helper
{
    /// <summary>
    /// Helper class for creating adaptive card attachments.
    /// </summary>
    public static class AdaptiveCardHelper
    {
        /// <summary>
        /// Gets an adaptive card attachment.
        /// </summary>
        /// <returns>An attachment containing the adaptive card.</returns>
        public static async Task<Attachment> GetAdaptiveCardAsync()
        {
            // Parse the JSON
            var result = AdaptiveCard.FromJson(await GetAdaptiveCardJsonAsync());

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = result.Card
            };
        }

        /// <summary>
        /// Gets the adaptive card JSON.
        /// </summary>
        /// <returns>The adaptive card JSON as a string.</returns>
        public static async Task<string> GetAdaptiveCardJsonAsync()
        {
            var path = Path.Combine(".", "Resources", "AdaptiveCard_TaskModule.json");
            return await File.ReadAllTextAsync(path);
        }
    }
}