// <copyright file="ITaskModuleResponseFactory.cs" company="Microsoft Corp.">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.PeoplePicker.Services
{
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;

    /// <summary>
    /// Task Module Response Contract.
    /// Task Module Response Factory Service renders different type of responses for task module.
    /// </summary>
    public interface ITaskModuleResponseFactory
    {
        /// <summary>
        /// Create Response with Adaptive card.
        /// </summary>
        /// <param name="card">Adaptive card attachment.</param>
        /// <returns>Returns Task Module Continue Response.</returns>
        TaskModuleContinueResponse CreateTaskModuleContinueResponse(Attachment card);

        /// <summary>
        /// Create Response with plain Text.
        /// </summary>
        /// <param name="message">Message that renders as plain text.</param>
        /// <returns>Returns Task Module Response.</returns>
        TaskModuleResponse CreateTaskModuleMessageResponse(string message);

        /// <summary>
        /// Create Response with Adaptive card.
        /// </summary>
        /// <param name="card">Adaptive card attachment.</param>
        /// <returns>Returns Task Module Response.</returns>
        TaskModuleResponse CreateTaskModuleCardResponse(Attachment card);
    }
}