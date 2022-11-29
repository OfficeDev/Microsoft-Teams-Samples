// <copyright file="ICard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace CallingBotSample.Interfaces
{
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Interface for cards.
    /// </summary>
    public interface ICard
    {
        Attachment GetWelcomeCardAttachment();
    }
}