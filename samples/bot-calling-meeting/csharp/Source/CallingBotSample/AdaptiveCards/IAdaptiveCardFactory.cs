// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Bot.Schema;

namespace CallingBotSample.AdaptiveCards
{
    public interface IAdaptiveCardFactory
    {
        Attachment CreateWelcomeCard(bool showJoinMeetingButton);
        Attachment CreateIncidentCard();
        Attachment CreateIncidentMeetingCard(string title, string callId, DateTime startTime, DateTime? endTime);
        Attachment CreateMeetingActionsCard(string? callId);
        Attachment CreatePeoplePickerCard(string choiceLabel, string action, string? callId, bool isMultiSelect = false);
    }
}
