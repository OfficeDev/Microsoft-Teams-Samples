// <copyright file="Claims.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Constants
{
    /// <summary>
    /// Claims contants.
    /// </summary>
    public static class Claims
    {
        /// <summary>
        /// Get the user id claim type.
        /// </summary>
        public const string UserId = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        /// <summary>
        /// Get the tenant id claim type.
        /// </summary>
        public const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";

        /// <summary>
        /// Get the name claim type.
        /// </summary>
        public const string Name = "name";

        /// <summary>
        /// Get the user principal name type.
        /// </summary>
        public const string UserPrincipalName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";
    }
}
