// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using JoinTeamByQR.Models;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

namespace JoinTeamByQR.Helpers
{
    /// <summary>
    /// This class is a wrapper for the Microsoft Graph API
    /// See: https://developer.microsoft.com/en-us/graph
    /// </summary>
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

        /// <summary>
        /// Get list of all teams present in the organization.
        /// </summary>
        public async Task<List<TeamData>> GetAllTeams()
        {
            var teamData = new List<TeamData>();
            var graphClient = GetAuthenticatedClient();

            try
            {
                var groups = await graphClient.Groups.GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Filter = "resourceProvisioningOptions/Any(x:x eq 'Team')";
                });

                foreach (var team in groups?.Value ?? new List<Group>())
                {
                    var teamDetails = new TeamData
                    {
                        TeamId = team.Id,
                        TeamName = team.DisplayName
                    };

                    teamData.Add(teamDetails);
                }

                return teamData.Take(5).ToList();
            }
            catch (ODataError odataError)
            {
                var errorCode = odataError.Error?.Code ?? "Unknown";
                var errorMessage = odataError.Error?.Message ?? odataError.Message;
                
                Console.WriteLine($"[GRAPH_ERROR] Code: {errorCode}, Message: {errorMessage}");
                
                // Provide more specific error messages
                if (errorCode == "Authorization_RequestDenied" || errorMessage?.Contains("Insufficient privileges") == true)
                {
                    throw new Exception($"Insufficient permissions to read groups/teams. Please ensure your OAuth connection has 'Group.Read.All' or 'Team.ReadBasic.All' scope configured. Error: {errorMessage}");
                }
                else if (errorCode == "InvalidAuthenticationToken")
                {
                    throw new Exception($"Invalid or expired authentication token. Please sign out and sign in again. Error: {errorMessage}");
                }
                
                throw new Exception($"Graph API error ({errorCode}): {errorMessage}");
            }
        }

        /// <summary>
        /// Add the user in the team.
        /// </summary>
        public async Task AddUserToTeam(string teamId, string userId)
        {
            var graphClient = GetAuthenticatedClient();
            
            try
            {
                var conversationMember = new AadUserConversationMember
                {
                    Roles = new List<String>()
                    {
                        "owner"
                    },
                    AdditionalData = new Dictionary<string, object>()
                    {
                        {"user@odata.bind", $"https://graph.microsoft.com/v1.0/users('{userId}')"}
                    }
                };

                await graphClient.Teams[teamId].Members.PostAsync(conversationMember);
            }
            catch (ODataError odataError)
            {
                var errorCode = odataError.Error?.Code ?? "Unknown";
                var errorMessage = odataError.Error?.Message ?? odataError.Message;
                
                Console.WriteLine($"[GRAPH_ERROR] Code: {errorCode}, Message: {errorMessage}");
                
                if (errorCode == "Authorization_RequestDenied" || errorMessage?.Contains("Insufficient privileges") == true)
                {
                    throw new Exception($"Insufficient permissions to add members to teams. Please ensure your OAuth connection has 'TeamMember.ReadWrite.All' scope configured. Error: {errorMessage}");
                }
                
                throw new Exception($"Graph API error ({errorCode}): {errorMessage}");
            }
        }

        /// <summary>
        /// Get an Authenticated Microsoft Graph client using the token issued to the user.
        /// </summary>
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
