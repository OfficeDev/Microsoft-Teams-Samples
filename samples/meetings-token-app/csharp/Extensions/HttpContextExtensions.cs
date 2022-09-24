// <copyright file="HttpContextExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Extensions
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// HttpContext extension class.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Get user id and tenant id from claims.
        /// </summary>
        /// <param name="httpContext">Http specific information.</param>
        /// <returns>the user id and tenant id.</returns>
        public static (string UserId, string TenantId) GetUserAndTenantIds(this HttpContext httpContext)
        {
            var userId = GetUserId(httpContext);
            var tenantId = httpContext.User.FindFirstValue(Constants.Claims.TenantId);
            return (userId, tenantId);
        }

        /// <summary>
        /// Get the user id from claims.
        /// </summary>
        /// <param name="httpContext">Http specific information.</param>
        /// <returns>the user id.</returns>
        public static string GetUserId(this HttpContext httpContext)
        {
            var userId = httpContext.User.FindFirstValue(Constants.Claims.UserId);
            return userId;
        }

        /// <summary>
        /// Get the user info from Http Context.
        /// </summary>
        /// <param name="httpContext">Http specific information.</param>
        /// <param name="meetingRole">user's meeting role.</param>
        /// <returns>the user info object.</returns>
        public static Models.UserInfo GetUserInfo(this HttpContext httpContext, string meetingRole)
        {
            var userId = GetUserId(httpContext);
            var name = httpContext.User.FindFirstValue(Constants.Claims.Name);
            var userPrincipalName = httpContext.User.FindFirstValue(Constants.Claims.UserPrincipalName);

            return new Models.UserInfo
            {
                AadObjectId = userId,
                Role = new Models.MeetingDetails
                {
                    MeetingRole = meetingRole,
                },
                Name = name,
                Email = userPrincipalName,
            };
        }
    }
}
