// <copyright file="InMemoryTenantInfoRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Repository
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Role in-memory storage service.
    /// </summary>
    public class InMemoryTenantInfoRepository : ITenantInfoRepository
    {
        private readonly ConcurrentDictionary<string, string> tenantIdToServiceUrlMap = new ConcurrentDictionary<string, string>();

        /// <inheritdoc/>
        public string GetServiceUrl(string tenantId)
        {
            return this.tenantIdToServiceUrlMap.GetValueOrDefault(tenantId);
        }

        /// <inheritdoc/>
        public void SetServiceUrl(string tenantId, string serviceUrl)
        {
            _ = this.tenantIdToServiceUrlMap.TryAdd(tenantId, serviceUrl);
        }
    }
}