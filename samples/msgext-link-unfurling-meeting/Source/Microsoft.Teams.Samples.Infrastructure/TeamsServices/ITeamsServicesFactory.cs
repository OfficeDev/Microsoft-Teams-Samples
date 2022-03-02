// <copyright file="ITeamsServicesFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.TeamsServices
{
    using Microsoft.Graph;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Services;

    /// <summary>
    /// Teams service factory contract.
    /// </summary>
    public interface ITeamsServicesFactory
    {
        /// <summary>
        /// Gets an implementation of meeting service.
        /// </summary>
        /// <param name="graphServiceClient">Graph service client.</param>
        /// <returns>Meeting service implementation.</returns>
        IMeetingsService GetMeetingsService(GraphServiceClient graphServiceClient);

        /// <summary>
        /// Gets an implementation of conversation service.
        /// </summary>
        /// <param name="graphServiceClient">Graph service client.</param>
        /// <returns>Conversation service implementation.</returns>
        IConversationService GetConversationService(GraphServiceClient graphServiceClient);
    }
}
