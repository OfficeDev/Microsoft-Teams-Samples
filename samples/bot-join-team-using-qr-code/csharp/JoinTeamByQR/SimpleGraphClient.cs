// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JoinTeamByQR.Models;
using Microsoft.Graph;

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

           var site =await graphClient.Groups
                             .Request()
                             .Filter("resourceProvisioningOptions/Any(x:x eq 'Team')")
                             .GetAsync();

            foreach (var team in site.CurrentPage)
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

            var response = await graphClient.Teams[teamId].Members
                             .Request()
                             .AddAsync(conversationMember);
        }

        // Get an Authenticated Microsoft Graph client using the token issued to the user.
        private GraphServiceClient GetAuthenticatedClient()
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", _token);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                        return Task.CompletedTask;
                    }));

            return graphClient;
        }
    }
}