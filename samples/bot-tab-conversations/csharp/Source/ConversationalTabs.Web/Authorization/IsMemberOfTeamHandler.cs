// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Web.Authorization;

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Exceptions;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Services;

public class IsMemberOfTeamHandler : AuthorizationHandler<IsMemberOfTeamRequirement, string>
{
    private readonly IUserTeamsService _userTeamsService;

    public IsMemberOfTeamHandler(IUserTeamsService userTeamsService)
    {
        _userTeamsService = userTeamsService;
    }

    protected override async Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context, IsMemberOfTeamRequirement requirement, string resource)
    {
        if (!context.User.Identity?.IsAuthenticated ?? false)
        {
            throw new ApiException(HttpStatusCode.Unauthorized, ErrorCode.Forbidden, "You are not authenticated.");
        }

        bool isValidChannel = await _userTeamsService.IsUserAMemberOfTeamAsync(resource);
        if (isValidChannel)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        throw new ApiException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, "You are not a member of this team.");
    }
}
