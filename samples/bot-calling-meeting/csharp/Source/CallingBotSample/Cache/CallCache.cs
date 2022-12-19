// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CallingBotSample.Cache
{
    public class CallCache : ICallCache
    {
        private readonly IMemoryCache cache;
        private readonly ILogger<CallCache> logger;

        private const string AT_LEAST_ONE_USER_JOINED = "atLeastOneUserJoined:";
        private const string IS_ESTABLISHED_KEY = "established:";

        public CallCache(IMemoryCache cache, ILogger<CallCache> logger)
        {
            this.cache = cache;
            this.logger = logger;
        }

        /// <inheritdoc />
        public bool GetIsEstablished(string callId)
        {
            return cache.Get<bool>(IS_ESTABLISHED_KEY + callId);
        }

        /// <inheritdoc />
        public void SetIsEstablished(string callId, bool isEstablished = true)
        {
            cache.Set(IS_ESTABLISHED_KEY + callId, isEstablished, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            });
        }

        /// <inheritdoc />
        public bool GetAtLeastOneUserJoined(string callId)
        {
            return cache.Get<bool>(AT_LEAST_ONE_USER_JOINED + callId);
        }

        /// <inheritdoc />
        public void SetAtLeastOneUserJoined(string callId, bool hasAtLeastOneUserJoined = true)
        {
            cache.Set(AT_LEAST_ONE_USER_JOINED + callId, hasAtLeastOneUserJoined, new MemoryCacheEntryOptions
            {
                // This 1 hour cache is sufficient for this sample.
                // If you are replicating this code, you might want to consider an alternative value which takes into account
                // the meeting's scheduled length.
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }
    }
}
