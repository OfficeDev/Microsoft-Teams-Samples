// Copyright (c) Microsoft Corporation. All rights reserved.

namespace CallingMediaBot.Domain.Factories;

using Microsoft.Bot.Schema;

public interface IAdaptiveCardFactory
{
    Attachment CreateWelcomeCard();
}
