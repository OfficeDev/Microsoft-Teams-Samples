// <copyright file="QnAExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.QnAService
{
    using System.Text.RegularExpressions;
    using System.Web;
    using HtmlAgilityPack;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Question Answer models extensions.
    /// </summary>
    public static class QnAExtensions
    {
        /// <summary>
        /// Removes html and user tags from the message.
        /// </summary>
        /// <param name="answer">Answer object.</param>
        /// <returns>Sanitized message.</returns>
        public static string GetSanitizedMessage(this Answer answer)
        {
            return SanitizeMessage(answer.Message);
        }

        /// <summary>
        /// Removes html and user tags from the message.
        /// </summary>
        /// <param name="question">Question object.</param>
        /// <returns>Sanitized message.</returns>
        public static string GetSanitizedMessage(this Question question)
        {
            return SanitizeMessage(question.Message);
        }

        /// <summary>
        /// Removes html and user tags from the message.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <returns>Sanitized  message.</returns>
        private static string SanitizeMessage(string message)
        {
            // Remove at mentions.
            message = Regex.Replace(message, "<at.*>(.*?)</at>", string.Empty, RegexOptions.IgnoreCase);

            // Remove HTML Tags.
            var doc = new HtmlDocument();
            doc.LoadHtml(message);
            message = HttpUtility.HtmlDecode(doc.DocumentNode.InnerText);

            // Remove escape sequences
            message = Regex.Replace(message, "\n|\t|\r", string.Empty);

            return message;
        }
    }
}
