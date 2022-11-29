// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Bot.Schema;

namespace CallingBotSample.AdaptiveCards
{
    public interface IAdaptiveCardFactory
    {
        Attachment CreateWelcomeCard();
    }
}
