// <copyright file="ITenantInfoRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Repository
{
    /// <summary>
    /// Tenant information repository.
    /// </summary>
    public interface ITenantInfoRepository
    {
        /// <summary>
        /// Get the service url for the given tenant.
        /// The service URL can vary between tenants, but will be the same for all users, channels and chats within a tenant.
        /// </summary>
        /// <param name="tenantId">The organization's tenant ID .</param>
        /// <returns>The service url.</returns>
        string GetServiceUrl(string tenantId);

        /// <summary>
        /// Set the service url for the given tenant.
        /// The service URL can vary between tenants, but will be the same for all users, channels and chats within a tenant.
        /// </summary>
        /// <param name="tenantId">The organization's tenant ID .</param>
        /// <param name="serviceUrl">The service url.</param>
        void SetServiceUrl(string tenantId, string serviceUrl);
    }
}