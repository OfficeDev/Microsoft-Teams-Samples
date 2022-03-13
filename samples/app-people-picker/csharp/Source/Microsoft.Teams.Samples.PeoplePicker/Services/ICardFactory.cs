// <copyright file="ICardFactory.cs" company="Microsoft Corp.">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.PeoplePicker.Services
{
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Card Factory contract.
    /// Card Factory Service exposes methods to render Adaptive Card or Adaptive Cards as attachment.
    /// </summary>
    public interface ICardFactory
    {
        /// <summary>
        /// Returns Attachment rendered as Adaptive Card.
        /// </summary>
        /// <param name="filePath">path of Adaptive Card template.</param>
        /// <returns>Attachment.</returns>
        public Attachment CreateAdaptiveCardAttachment(string filePath);

        /// <summary>
        ///  Returns Attachment rendered as Adaptive Card. Additionaly takes in Object to be rendered into Card.
        /// </summary>
        /// <param name="filePath">path of Adaptive Card template.</param>
        /// <param name="dataObj">Object to be rendered into Card.</param>
        /// <returns>Attachment.</returns>
        public Attachment CreateAdaptiveCardAttachement(string filePath, object dataObj);
    }
}
