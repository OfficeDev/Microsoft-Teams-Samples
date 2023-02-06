// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Infrastructure.GraphService;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Services;

/// <inheritdoc  />
public class UsersTeamService : GraphServiceHelper, IUserTeamsService
{
    private readonly GraphServiceClient _graphServiceClient;

    public UsersTeamService(
        GraphServiceClient graphServiceClient,
        ILogger<UsersTeamService> logger) : base(logger)
    {
        _graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
    }

    // We are using Team membership to determine if a user is a channel member.
    // Except for private channels, every member of a Team is a member of all channels.
    // Currently, private channels do not support bots, so it's not possible to install our app in private channels.
    // So we are comfortable with the API call below.
    public async Task<IEnumerable<Team>> GetUsersTeamsAsync()
    {
        try
        {
            // IMPORTANT: A full solution should consider caching the response from Graph to prevent hitting a rate limit.
            var result = await _graphServiceClient.Me.JoinedTeams
                .Request()
                .GetAsync()
                .ConfigureAwait(false);

            return result.Select(row => row);
        }
        catch (Exception ex)
        {
            throw HandleGraphExceptions(ex);
        }
    }

    public async Task<bool> IsUserAMemberOfTeamAsync(string aadGroupId)
    {
        // NOTE: There is a number of APIs you can use to get Team membership, an alternative to JoinedTeams used above
        // is 'List group transitive memberOf' (https://docs.microsoft.com/en-us/graph/api/group-list-transitivememberof)
        var usersTeams = await GetUsersTeamsAsync().ConfigureAwait(false);
        return usersTeams.Any(team => team.Id == aadGroupId);
    }
}
