// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using global::AdaptiveCards;
using global::AdaptiveCards.Templating;
using Microsoft.Bot.Schema;

namespace CallingBotSample.AdaptiveCards
{
    public class AdaptiveCardFactory : IAdaptiveCardFactory
    {
        /// <summary>
        /// Date Time must be in RFC 3389 format https://learn.microsoft.com/en-us/adaptive-cards/authoring-cards/text-features#datetime-function-rules
        /// </summary>
        public const string DateTimeStringFormat = "yyyy-MM-dd'T'HH:mm:ssK";

        /// <inheritdoc />
        public Attachment CreateWelcomeCard(bool showJoinMeetingButton)
        {
            var template = GetCardTemplate("WelcomeCard.json");

            var serializedJson = template.Expand(new { showJoinMeetingButton });
            return CreateAttachment(serializedJson);
        }

        /// <inheritdoc />
        public Attachment CreateIncidentCard()
        {
            var template = GetCardTemplate("CreateIncidentCard.json");

            var serializedJson = template.Expand(new { });
            return CreateAttachment(serializedJson);
        }

        /// <inheritdoc />
        public Attachment CreateIncidentMeetingCard(string title, string callId, DateTime startTime, DateTime? endTime)
        {
            var template = GetCardTemplate("IncidentMeetingActionsCard.json");

            var serializedJson = template.Expand(new
            {
                title,
                callId,
                startTime = startTime.ToString(DateTimeStringFormat),
                endTime = endTime?.ToString(DateTimeStringFormat) ?? string.Empty
            });
            return CreateAttachment(serializedJson);
        }

        /// <inheritdoc />
        public Attachment CreateMeetingActionsCard(string? callId)
        {
            var template = GetCardTemplate("MeetingActions.json");

            var serializedJson = template.Expand(new { callId = callId ?? string.Empty });
            return CreateAttachment(serializedJson);
        }

        /// <inheritdoc />
        public Attachment CreatePeoplePickerCard(string choiceLabel, string action, string? callId, bool isMultiSelect = false)
        {
            var template = GetCardTemplate("PeoplePicker.json");

            var serializedJson = template.Expand(new
            {
                choiceLabel,
                action,
                callId = callId ?? string.Empty,
                isMultiSelect
            });
            return CreateAttachment(serializedJson);

        }

        private AdaptiveCardTemplate GetCardTemplate(string fileName)
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", fileName);
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
