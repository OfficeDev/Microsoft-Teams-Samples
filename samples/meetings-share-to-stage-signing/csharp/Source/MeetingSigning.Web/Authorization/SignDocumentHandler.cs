// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Authorization
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    /// <summary>
    /// Post answer handler.
    /// </summary>
    public class SignDocumentHandler : AuthorizationHandler<SignDocumentRequirement, Signature>
    {
        /// <inheritdoc/>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SignDocumentRequirement requirement, Signature resource)
        {
            // Ensure user is authenticated.
            if (!context.User.Identity?.IsAuthenticated ?? false)
            {
                throw new ApiException(HttpStatusCode.Unauthorized, ErrorCode.Forbidden, "You are not authenticated.");
            }

            // Is User a signer
            string? userId = context.User.GetUserId();
            string? userEmail = context.User.GetUserEmail();

            if ((userId != null && resource.Signer.UserId == userId) ||
                (userEmail != null && resource.Signer.Email == userEmail))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            throw new ApiException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, "You are not authorized to sign this document.");
        }
    }
}
