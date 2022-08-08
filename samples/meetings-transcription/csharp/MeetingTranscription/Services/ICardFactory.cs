// <copyright file="ICardFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MeetingTranscription.Services
{
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Card Factory Service exposes methods to create Adaptive Card or Adaptive Cards as attachment.
    /// </summary>
    public interface ICardFactory
    {
        /// <summary>
        ///  Returns Attachment rendered as Adaptive Card. Additionally takes in object to be rendered into Card.
        /// </summary>
        /// <param name="dataObj">Object to be rendered into Card.</param>
        /// <returns>Adaptive card attachment.</returns>
        public Attachment CreateAdaptiveCardAttachement(object dataObj);

        /// <summary>
        /// Returns Attachment rendered as Not Found Adaptive Card.
        /// </summary>
        /// <returns>Adaptive card attachment.</returns>
        public Attachment CreateNotFoundCardAttachement();
    }
}
