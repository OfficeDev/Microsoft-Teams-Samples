// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using CallingBotSample.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CallingBotSample.Cache
{
    public class IncidentCache : IIncidentCache
    {
        private readonly IMemoryCache cache;
        private readonly ILogger<IncidentCache> logger;
        private const string IncidentKey = "incident:";

        public IncidentCache(IMemoryCache cache, ILogger<IncidentCache> logger)
        {
            this.cache = cache;
            this.logger = logger;
        }

        /// <inheritdoc />
        public bool TryGetValue(string callId, out IncidentDetails incidentDetails)
        {
            return cache.TryGetValue<IncidentDetails>(IncidentKey + callId, out incidentDetails);
        }

        /// <inheritdoc />
        public void Set(string callId, IncidentDetails incidentDetails)
        {
            cache.Set(IncidentKey + callId, incidentDetails, new MemoryCacheEntryOptions
            {
                // This 1 hour cache is sufficient for this sample.
                // If you are replicating this code, you might want to consider an alternative value which takes into account
                // the meeting's scheduled length.
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }
    }
}
