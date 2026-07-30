// <copyright file="CardFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MeetingAttendance.Services
{
    using System.IO;
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using MeetingAttendance.Models;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    /// <summary>
    /// Returns CardAttachment.
    /// </summary>
    public static class CardFactory
    {
        /// <summary>
        /// Gets meeting attendance report card.
        /// </summary>
        /// <param name="dataObj">Meeting attendance model</param>
        /// <returns>Attendance report card</returns>
        public static Attachment GetMeetingReportAdaptiveCard(MeetingAttendanceSummary dataObj)
        {
            var cardJSON = File.ReadAllText(Path.Combine(".", "Resources", "AttendanceSummaryCard.json"));
            if (dataObj != null)
            {
                AdaptiveCardTemplate template = new AdaptiveCardTemplate(cardJSON);
                cardJSON = template.Expand(dataObj);
            }

            var adaptiveCardAttachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(cardJSON),
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Gets welcome card.
        /// </summary>
        /// <returns>Welcome card.</returns>
        public static Attachment GetWelcomeCard()
        {
            var cardJSON = File.ReadAllText(Path.Combine(".", "Resources", "WelcomeCard.json"));
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(cardJSON),
            };

            return adaptiveCardAttachment;
        }
    }
}
