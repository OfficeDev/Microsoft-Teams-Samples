// <copyright file="ISubscriptionProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using TabActivityFeed.Models;

    /// <summary>
    /// Interface defining a provider for subscription related operations.
    /// </summary>
    public interface ISubscriptionProvider
    {
        /// <summary>
        /// Creates a subscription for the teams scope.
        /// </summary>
        /// <returns>A subscription.</returns>
        Task<Subscription> CreateTeamsSubscriptionAsync();

        /// <summary>
        /// Creates a subscription for the chat scope.
        /// </summary>
        /// <returns>A subscription.</returns>
        Task<Subscription> CreateChatSubscriptionAsync();
    }
}
