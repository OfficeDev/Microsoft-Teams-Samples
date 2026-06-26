// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using JoinTeamByQR.Models;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace JoinTeamByQR
{
    // This class is a wrapper for the Microsoft Graph API
    // See: https://developer.microsoft.com/en-us/graph
    public class SimpleGraphClient
    {
        private readonly string _token;

        public SimpleGraphClient(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            _token = token;
        }

        // Get list of all teams present in thee organization.
        public async Task<List<TeamData>> GetAllTeams()
        {
            var teamData = new List<TeamData>();
            var graphClient = GetAuthenticatedClient();

            var site = await graphClient.Groups.GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Filter = "resourceProvisioningOptions/Any(x:x eq 'Team')";
            });

            foreach (var team in site?.Value ?? new List<Group>())
            {
                var teamDetails = new TeamData
                {
                    teamId = team.Id,
                    teamName = team.DisplayName
                };

                teamData.Add(teamDetails);
            }

            return teamData.Take(5).ToList();
        }

        // Add the user in the team.
        public async void AddUserToTeam(string teamId, string userId)
        {
            var graphClient = GetAuthenticatedClient();
            var conversationMember = new AadUserConversationMember
            {
                Roles = new List<String>()
                {
                    "owner"
                },
                AdditionalData = new Dictionary<string, object>()
                {
                    {"user@odata.bind", "https://graph.microsoft.com/v1.0/users('"+userId+"')"}
                }
            };

            await graphClient.Teams[teamId].Members.PostAsync(conversationMember);

        }

        // Get an Authenticated Microsoft Graph client using the token issued to the user.
        private GraphServiceClient GetAuthenticatedClient()
        {
            var tokenCredential = new MyAccessTokenProvider(_token);
            return new GraphServiceClient(tokenCredential);
        }

        public class MyAccessTokenProvider : TokenCredential
        {
            private readonly string _token;

            public MyAccessTokenProvider(string token)
            {
                _token = token;
            }

            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                return new AccessToken(_token, DateTimeOffset.UtcNow.AddHours(1));
            }

            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                return new ValueTask<AccessToken>(new AccessToken(_token, DateTimeOffset.UtcNow.AddHours(1)));
            }
        }
    }
}