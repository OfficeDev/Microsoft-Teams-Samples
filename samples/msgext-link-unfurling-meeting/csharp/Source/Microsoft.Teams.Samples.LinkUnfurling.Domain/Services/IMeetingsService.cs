// <copyright file="IMeetingsService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Domain.Services
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;

    /// <summary>
    /// Meetings service contract.
    ///
    /// Meeting service exploses methods to create and retrieve online meetings.
    /// </summary>
    public interface IMeetingsService
    {
        /// <summary>
        /// Creates an online meeting.
        /// </summary>
        /// <param name="meeting">Meeting object.</param>
        /// <returns><see cref="Meeting"/> object.</returns>
        Task<Meeting> CreateOnlineMeetingAsync(Meeting meeting);

        /// <summary>
        /// Gets online meeting object.
        /// </summary>
        /// <param name="externalId">External id for the meeting.</param>
        /// <returns>Meeting object.</returns>
        Task<Meeting> GetOnlineMeetingAsync(string externalId);
    }
}
