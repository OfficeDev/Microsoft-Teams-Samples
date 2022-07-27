// <copyright file="IMeetingResponseCache.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Web.ResponseCache
{
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;
    using Microsoft.Teams.Samples.LinkUnfurling.Web.Controllers;

    /// <summary>
    /// Response cache contract.
    ///
    /// Provides methods to cache and retrieve a setup meeting response object against its external id.
    /// </summary>
    public interface IMeetingResponseCache
    {
        /// <summary>
        /// Gets meeting response cache for a given <see cref="Meeting.ExternalId"/> linked to it.
        /// </summary>
        /// <param name="externalId">Meeting's external id.</param>
        /// <returns><see cref="Meeting"/>.</returns>
        SetupResponse GetMeetingResponseForExternalId(string externalId);

        /// <summary>
        /// Adds meeting response object to cache.
        /// </summary>
        /// <param name="externalId">Meeting's external id.</param>
        /// <param name="meeting">Meeting object.</param>
        void AddMeetingResponseForExternalId(string externalId, SetupResponse meeting);
    }
}