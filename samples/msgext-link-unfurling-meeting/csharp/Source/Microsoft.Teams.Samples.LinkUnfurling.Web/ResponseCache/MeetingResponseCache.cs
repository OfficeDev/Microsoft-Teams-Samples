// <copyright file="MeetingResponseCache.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Web.ResponseCache
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Teams.Samples.LinkUnfurling.Web.Controllers;

    /// <summary>
    /// Meeting response cache.
    ///
    /// Note: This sample caches the response indefinitely as the join meeting link and tab id do not expire.
    /// For production scenarios, you should consider persisting the meeting data in storage and adding a lifetime for the cache.
    /// </summary>
    internal class MeetingResponseCache : IMeetingResponseCache
    {
        private readonly Dictionary<string, SetupResponse> responseDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingResponseCache"/> class.
        /// </summary>
        public MeetingResponseCache()
        {
            this.responseDictionary = new Dictionary<string, SetupResponse>();
        }

        /// <inheritdoc/>
        SetupResponse IMeetingResponseCache.GetMeetingResponseForExternalId(string externalId)
        {
            if (string.IsNullOrEmpty(externalId))
            {
                throw new ArgumentException($"'{nameof(externalId)}' cannot be null or empty.", nameof(externalId));
            }

            if (this.responseDictionary.ContainsKey(externalId))
            {
                return this.responseDictionary.GetValueOrDefault(externalId);
            }

            return null;
        }

        /// <inheritdoc/>
        public void AddMeetingResponseForExternalId(string externalId, SetupResponse meeting)
        {
            if (string.IsNullOrEmpty(externalId))
            {
                throw new ArgumentException($"'{nameof(externalId)}' cannot be null or empty.", nameof(externalId));
            }

            if (meeting is null)
            {
                throw new ArgumentNullException(nameof(meeting));
            }

            this.responseDictionary.TryAdd(externalId, meeting);
        }
    }
}
