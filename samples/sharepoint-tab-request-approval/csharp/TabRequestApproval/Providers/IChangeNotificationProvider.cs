// <copyright file="IChangeNotificationProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
    using System.Threading.Tasks;
    using TabActivityFeed.Models;

    /// <summary>
    /// Interface for the change notification provider.
    /// </summary>
    public interface IChangeNotificationProvider
    {
        /// <summary>
        /// Processes information related to teams subscriptions.
        /// </summary>
        /// <param name="pagedNotificationPayload">Represents the notification payload from Graph API.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ProcessTeamsChangeNotificationAsync(PagedNotificationPayload pagedNotificationPayload);

        /// <summary>
        /// Processes information related to chat subscriptions.
        /// </summary>
        /// <param name="pagedNotificationPayload">Represents the notification payload from Graph API.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ProcessChatsChangeNotificationAsync(PagedNotificationPayload pagedNotificationPayload);
    }
}
