// <copyright file="IMeetingSetup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Domain.OnlineMeetings
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;

    /// <summary>
    /// Meeting setup contract.
    ///
    /// Exposes methods to setup a meeting to review a resource.
    /// </summary>
    public interface IMeetingSetup
    {
        /// <summary>
        /// Creates an online meeting if its not already created for to review the resource in the conversation context.
        ///
        /// Installs application, adds a tab and returns meeting details.
        /// </summary>
        /// <param name="meeting">Meeting request object.</param>
        /// <param name="conversationContext">Conversation context.</param>
        /// <param name="resource">Resource object.</param>
        /// <param name="userId">Requesting user's AAD id.</param>
        /// <returns>Join meeting url and tab id.</returns>
        Task<(string, string)> SetupReviewMeetingAsync(Meeting meeting, ConversationContext conversationContext, Resource resource, string userId);
    }
}
