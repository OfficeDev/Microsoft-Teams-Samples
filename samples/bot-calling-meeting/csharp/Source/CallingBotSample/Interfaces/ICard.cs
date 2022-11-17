// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Bot.Schema;

namespace CallingBotSample.Interfaces
{
    /// <summary>
    /// Interface for cards.
    /// </summary>
    public interface ICard
    {
        Attachment GetWelcomeCardAttachment();
    }
}
