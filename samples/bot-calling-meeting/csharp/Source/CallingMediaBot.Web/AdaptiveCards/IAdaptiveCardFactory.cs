// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingMediaBot.Web.AdaptiveCards;
using Microsoft.Bot.Schema;

public interface IAdaptiveCardFactory
{
    Attachment CreateWelcomeCard();
    Attachment CreateMeetingActionsCard();
    Attachment CreatePeoplePickerCard(string choiceLabel, string action, bool isMultiSelect = false);
}
