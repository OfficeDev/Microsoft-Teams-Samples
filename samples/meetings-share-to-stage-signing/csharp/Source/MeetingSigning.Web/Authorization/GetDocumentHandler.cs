// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Authorization
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    /// <summary>
    /// Get Document handler.
    /// </summary>
    public class GetDocumentHandler : AuthorizationHandler<GetDocumentRequirement, Document>
    {
        /// <inheritdoc/>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GetDocumentRequirement requirement, Document resource)
        {
            string userId = context.User.GetUserId();
            if (resource.OwnerId == userId ||
                resource.Signatures.Any(s => s.Signer.UserId == userId) ||
                resource.Viewers.Any(v => v.Observer.UserId == userId))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            throw new ApiException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, "You are not authorized to view this document.");
        }
    }
}
